using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugins.Slack
{
    public class SlackController : EssentialsBridgeableDevice
    {
        private const string SlackApiUrl = "https://slack.com/api/chat.postMessage";

        private string WebhookUrl;
        private string BotToken;
        internal string DefaultUsername;
        private string DefaultChannel;
        internal string DefaultIconEmoji;

        private readonly HttpClient _httpClient;

        // Webhook state
        private string _pendingMessageWebhook;
        private bool _isBusyWebhook;
        private bool _lastSendSuccessfulWebhook;
        private string _currentChannelWebhook;

        // Bot state
        private string _pendingMessageBot;
        private bool _isBusyBot;
        private bool _lastSendSuccessfulBot;
        private string _currentChannelBot;

        /// <summary>
        /// Indicates if webhook URL is configured
        /// </summary>
        public bool WebhookConfigured => !string.IsNullOrEmpty(WebhookUrl);

        /// <summary>
        /// Indicates if bot token is configured
        /// </summary>
        public bool BotTokenConfigured => !string.IsNullOrEmpty(BotToken);

        #region Webhook Feedbacks

        /// <summary>
        /// Feedback indicating if the webhook is currently sending a message
        /// </summary>
        public BoolFeedback IsBusyFeedback { get; private set; }

        /// <summary>
        /// Feedback indicating the last webhook message send was successful
        /// </summary>
        public BoolFeedback LastSendSuccessfulFeedback { get; private set; }

        /// <summary>
        /// Feedback for the current webhook channel (override or default)
        /// </summary>
        public StringFeedback CurrentChannelFeedback { get; private set; }

        #endregion

        #region Bot Feedbacks

        /// <summary>
        /// Feedback indicating if the bot is currently sending a message
        /// </summary>
        public BoolFeedback IsBusyBotFeedback { get; private set; }

        /// <summary>
        /// Feedback indicating the last bot message send was successful
        /// </summary>
        public BoolFeedback LastSendSuccessfulBotFeedback { get; private set; }

        /// <summary>
        /// Feedback for the current bot channel/user (override or default)
        /// </summary>
        public StringFeedback CurrentChannelBotFeedback { get; private set; }

        #endregion

        public SlackController(string key, string name, SlackPropertiesConfig propertiesConfig)
            : base(key, name)
        {
            //_config = propertiesConfig;
            WebhookUrl = propertiesConfig.WebhookUrl;
            BotToken = propertiesConfig.BotToken;
            DefaultChannel = propertiesConfig.DefaultChannel;
            DefaultUsername = propertiesConfig.DefaultUsername;
            DefaultIconEmoji = propertiesConfig.DefaultIconEmoji;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Webhook feedbacks
            IsBusyFeedback = new BoolFeedback(key + "-IsBusy", () => _isBusyWebhook);
            LastSendSuccessfulFeedback = new BoolFeedback(key + "-LastSendSuccessful", () => _lastSendSuccessfulWebhook);
            CurrentChannelFeedback = new StringFeedback(key + "-Channel", () => GetCurrentChannelWebhook());

            // Bot feedbacks
            IsBusyBotFeedback = new BoolFeedback(key + "-IsBusyBot", () => _isBusyBot);
            LastSendSuccessfulBotFeedback = new BoolFeedback(key + "-LastSendSuccessfulBot", () => _lastSendSuccessfulBot);
            CurrentChannelBotFeedback = new StringFeedback(key + "-ChannelBot", () => GetCurrentChannelBot());
        }

        public override void Initialize()
        {
            base.Initialize();

            if (BotTokenConfigured)
            {
                this.LogDebug("Slack Bot Token is configured");
            }

            if (WebhookConfigured)
            {
                this.LogDebug("Slack Webhook URL is configured");
            }

            if (!BotTokenConfigured && !WebhookConfigured)
            {
                this.LogWarning("Neither Bot Token nor Webhook URL is configured");
            }
        }


        #region Webhook Methods

        /// <summary>
        /// Gets the current webhook channel (override if set, otherwise default)
        /// </summary>
        public string GetCurrentChannelWebhook()
        {
            return !string.IsNullOrEmpty(_currentChannelWebhook) ? _currentChannelWebhook : DefaultChannel ?? string.Empty;
        }

        /// <summary>
        /// Sets the message to be sent on the next webhook trigger
        /// </summary>
        /// <param name="message">The message text</param>
        public void SetMessageWebhook(string message)
        {
            _pendingMessageWebhook = message;
            this.LogDebug("Webhook message set: {0}", message);
        }

        /// <summary>
        /// Sends the pending message to Slack via webhook
        /// </summary>
        public void SendMessageWebhook()
        {
            if (string.IsNullOrEmpty(_pendingMessageWebhook))
            {
                this.LogWarning("No webhook message to send");
                return;
            }

            SendWebhookMessageAsync(_pendingMessageWebhook);
        }

        /// <summary>
        /// Sends a message directly to Slack via webhook without setting it first
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessageDirectWebhook(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                this.LogWarning("Cannot send empty webhook message");
                return;
            }

            SendWebhookMessageAsync(message);
        }

        /// <summary>
        /// Sets the webhook channel override
        /// </summary>
        /// <param name="channel">The channel to send messages to</param>
        public void SetChannelWebhook(string channel)
        {
            _currentChannelWebhook = channel;
            this.LogDebug("Webhook channel override set to: {0}", channel);
            CurrentChannelFeedback.FireUpdate();
        }

        /// <summary>
        /// Resets the webhook channel to the default configured channel
        /// </summary>
        public void ResetChannelWebhook()
        {
            _currentChannelWebhook = null;
            this.LogDebug("Webhook channel reset to default: {0}", DefaultChannel ?? "(none)");
            CurrentChannelFeedback.FireUpdate();
        }

        #endregion

        #region Bot Methods

        /// <summary>
        /// Gets the current bot channel (override if set, otherwise default)
        /// </summary>
        public string GetCurrentChannelBot()
        {
            return !string.IsNullOrEmpty(_currentChannelBot) ? _currentChannelBot : DefaultChannel ?? string.Empty;
        }

        /// <summary>
        /// Sets the message to be sent on the next bot trigger
        /// </summary>
        /// <param name="message">The message text</param>
        public void SetMessageBot(string message)
        {
            _pendingMessageBot = message;
            this.LogDebug("Bot message set: {0}", message);
        }

        /// <summary>
        /// Sends the pending message to Slack via bot
        /// </summary>
        public void SendMessageBot()
        {
            if (string.IsNullOrEmpty(_pendingMessageBot))
            {
                this.LogWarning("No bot message to send");
                return;
            }

            SendBotMessageAsync(_pendingMessageBot);
        }

        /// <summary>
        /// Sends a message directly to Slack via bot without setting it first
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessageDirectBot(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                this.LogWarning("Cannot send empty bot message");
                return;
            }

            SendBotMessageAsync(message);
        }

        /// <summary>
        /// Sets the bot channel/user override
        /// </summary>
        /// <param name="channel">The channel or user to send messages to</param>
        public void SetChannelBot(string channel)
        {
            _currentChannelBot = channel;
            this.LogDebug("Bot channel override set to: {0}", channel);
            CurrentChannelBotFeedback.FireUpdate();
        }

        /// <summary>
        /// Resets the bot channel to the default configured channel
        /// </summary>
        public void ResetChannelBot()
        {
            _currentChannelBot = null;
            this.LogDebug("Bot channel reset to default: {0}", DefaultChannel ?? "(none)");
            CurrentChannelBotFeedback.FireUpdate();
        }

        #endregion

        #region Webhook Async Methods

        private async void SendWebhookMessageAsync(string message)
        {
            if (!WebhookConfigured)
            {
                this.LogError("Webhook URL is not configured");
                _lastSendSuccessfulWebhook = false;
                LastSendSuccessfulFeedback.FireUpdate();
                return;
            }

            if (_isBusyWebhook)
            {
                this.LogWarning("Webhook is busy sending a message, please wait");
                return;
            }

            _isBusyWebhook = true;
            IsBusyFeedback.FireUpdate();

            try
            {
                var payload = new SlackMessagePayload
                {
                    Text = message,
                    Channel = GetCurrentChannelWebhook(),
                    Username = DefaultUsername,
                    IconEmoji = DefaultIconEmoji
                };

                var json = JsonConvert.SerializeObject(payload);
                this.LogDebug("Sending Slack message via webhook: {0}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(WebhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    this.LogInformation("Message sent successfully via webhook");
                    _lastSendSuccessfulWebhook = true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    this.LogError("Failed to send webhook message. Status: {0}, Response: {1}", response.StatusCode, responseBody);
                    _lastSendSuccessfulWebhook = false;
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception sending webhook message: {0}", ex.Message);
                _lastSendSuccessfulWebhook = false;
            }
            finally
            {
                _isBusyWebhook = false;
                IsBusyFeedback.FireUpdate();
                LastSendSuccessfulFeedback.FireUpdate();
            }
        }

        #endregion

        #region Bot Async Methods

        private async void SendBotMessageAsync(string message)
        {
            if (!BotTokenConfigured)
            {
                this.LogError("Bot Token is not configured");
                _lastSendSuccessfulBot = false;
                LastSendSuccessfulBotFeedback.FireUpdate();
                return;
            }

            var channel = GetCurrentChannelBot();
            if (string.IsNullOrEmpty(channel))
            {
                this.LogError("Channel is required when using Bot Token");
                _lastSendSuccessfulBot = false;
                LastSendSuccessfulBotFeedback.FireUpdate();
                return;
            }

            if (_isBusyBot)
            {
                this.LogWarning("Bot is busy sending a message, please wait");
                return;
            }

            _isBusyBot = true;
            IsBusyBotFeedback.FireUpdate();

            try
            {
                var payload = new SlackBotApiPayload
                {
                    Channel = channel,
                    Text = message,
                    Username = DefaultUsername,
                    IconEmoji = DefaultIconEmoji
                };

                var json = JsonConvert.SerializeObject(payload);
                this.LogDebug("Sending Slack message via Bot API to {0}: {1}", channel, message);

                var request = new HttpRequestMessage(HttpMethod.Post, SlackApiUrl);
                request.Headers.Add("Authorization", "Bearer " + BotToken);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<SlackApiResponse>(responseBody);
                    if (apiResponse != null && apiResponse.Ok)
                    {
                        this.LogInformation("Message sent successfully via Bot API");
                        _lastSendSuccessfulBot = true;
                    }
                    else
                    {
                        this.LogError("Slack API error: {0}", apiResponse?.Error ?? "Unknown error");
                        _lastSendSuccessfulBot = false;
                    }
                }
                else
                {
                    this.LogError("Failed to send bot message. Status: {0}, Response: {1}", response.StatusCode, responseBody);
                    _lastSendSuccessfulBot = false;
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception sending bot message: {0}", ex.Message);
                _lastSendSuccessfulBot = false;
            }
            finally
            {
                _isBusyBot = false;
                IsBusyBotFeedback.FireUpdate();
                LastSendSuccessfulBotFeedback.FireUpdate();
            }
        }

        #endregion

        #region IBridgeAdvanced Members

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new SlackBridgeJoinMap(joinStart);

            // This adds the join map to the collection on the bridge
            bridge?.AddJoinMap(Key, joinMap);

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            this.LogDebug("Linking to Trilist {id}", trilist.ID.ToString("X"));
            this.LogInformation("Linking to Bridge Type {type}", GetType().Name);

            // Webhook Digital joins
            trilist.SetSigTrueAction(joinMap.SendMessageWebhook.JoinNumber, SendMessageWebhook);
            trilist.SetSigTrueAction(joinMap.ResetChannelWebhook.JoinNumber, ResetChannelWebhook);
            IsBusyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsBusyWebhook.JoinNumber]);
            LastSendSuccessfulFeedback.LinkInputSig(trilist.BooleanInput[joinMap.LastSendSuccessfulWebhook.JoinNumber]);

            // Bot Digital joins
            trilist.SetSigTrueAction(joinMap.SendMessageBot.JoinNumber, SendMessageBot);
            trilist.SetSigTrueAction(joinMap.ResetChannelBot.JoinNumber, ResetChannelBot);
            IsBusyBotFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsBusyBot.JoinNumber]);
            LastSendSuccessfulBotFeedback.LinkInputSig(trilist.BooleanInput[joinMap.LastSendSuccessfulBot.JoinNumber]);

            // Webhook Serial joins
            trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
            trilist.SetStringSigAction(joinMap.MessageTextWebhook.JoinNumber, SetMessageWebhook);
            trilist.SetStringSigAction(joinMap.SendMessageDirectWebhook.JoinNumber, SendMessageDirectWebhook);
            trilist.SetStringSigAction(joinMap.ChannelWebhook.JoinNumber, SetChannelWebhook);
            CurrentChannelFeedback.LinkInputSig(trilist.StringInput[joinMap.ChannelWebhook.JoinNumber]);

            // Bot Serial joins
            trilist.SetStringSigAction(joinMap.MessageTextBot.JoinNumber, SetMessageBot);
            trilist.SetStringSigAction(joinMap.SendMessageDirectBot.JoinNumber, SendMessageDirectBot);
            trilist.SetStringSigAction(joinMap.ChannelBot.JoinNumber, SetChannelBot);
            CurrentChannelBotFeedback.LinkInputSig(trilist.StringInput[joinMap.ChannelBot.JoinNumber]);

            trilist.OnlineStatusChange += (sender, args) =>
            {
                if (!args.DeviceOnLine) return;

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

                // Webhook feedbacks
                IsBusyFeedback.FireUpdate();
                LastSendSuccessfulFeedback.FireUpdate();
                CurrentChannelFeedback.FireUpdate();

                // Bot feedbacks
                IsBusyBotFeedback.FireUpdate();
                LastSendSuccessfulBotFeedback.FireUpdate();
                CurrentChannelBotFeedback.FireUpdate();
            };
        }

        #endregion
    }

    /// <summary>
    /// Slack message payload structure for webhooks
    /// </summary>
    internal class SlackMessagePayload
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
        public string Channel { get; set; }

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("icon_emoji", NullValueHandling = NullValueHandling.Ignore)]
        public string IconEmoji { get; set; }
    }

    /// <summary>
    /// Slack Bot API payload structure
    /// </summary>
    internal class SlackBotApiPayload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("icon_emoji", NullValueHandling = NullValueHandling.Ignore)]
        public string IconEmoji { get; set; }
    }

    /// <summary>
    /// Slack API response structure
    /// </summary>
    internal class SlackApiResponse
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}