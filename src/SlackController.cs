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
        private const string slackApiUrl = "https://slack.com/api/chat.postMessage";

        private readonly string webhookUrl;
        private readonly string botToken;
        internal readonly string defaultUsername;
        internal readonly string defaultChannel;
        internal readonly string defaultIconEmoji;

        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Webhook state
        private string pendingMessageWebhook;
        private bool isBusyWebhook;
        private bool lastSendSuccessfulWebhook;
        private string currentChannelWebhook;

        // Bot state
        private string pendingMessageBot;
        private bool isBusyBot;
        private bool lastSendSuccessfulBot;
        private string currentChannelBot;

        /// <summary>
        /// Indicates if webhook URL is configured
        /// </summary>
        public bool WebhookConfigured => !string.IsNullOrEmpty(webhookUrl);

        /// <summary>
        /// Indicates if bot token is configured
        /// </summary>
        public bool BotTokenConfigured => !string.IsNullOrEmpty(botToken);

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
            webhookUrl = propertiesConfig.WebhookUrl;
            botToken = propertiesConfig.BotToken;
            defaultChannel = propertiesConfig.DefaultChannel;
            defaultUsername = propertiesConfig.DefaultUsername;
            defaultIconEmoji = propertiesConfig.DefaultIconEmoji;

            // Webhook feedbacks
            IsBusyFeedback = new BoolFeedback(key + "-IsBusy", () => isBusyWebhook);
            LastSendSuccessfulFeedback = new BoolFeedback(key + "-LastSendSuccessful", () => lastSendSuccessfulWebhook);
            CurrentChannelFeedback = new StringFeedback(key + "-Channel", () => GetCurrentChannelWebhook());

            // Bot feedbacks
            IsBusyBotFeedback = new BoolFeedback(key + "-IsBusyBot", () => isBusyBot);
            LastSendSuccessfulBotFeedback = new BoolFeedback(key + "-LastSendSuccessfulBot", () => lastSendSuccessfulBot);
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
            return !string.IsNullOrEmpty(currentChannelWebhook) ? currentChannelWebhook : defaultChannel ?? string.Empty;
        }

        /// <summary>
        /// Sets the message to be sent on the next webhook trigger
        /// </summary>
        /// <param name="message">The message text</param>
        public void SetMessageWebhook(string message)
        {
            pendingMessageWebhook = message;
            this.LogDebug("Webhook message set: {0}", message);
        }

        /// <summary>
        /// Sends the pending message to Slack via webhook
        /// </summary>
        public void SendMessageWebhook()
        {
            if (string.IsNullOrEmpty(pendingMessageWebhook))
            {
                this.LogWarning("No webhook message to send");
                return;
            }

            SendWebhookMessageAsync(pendingMessageWebhook);
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
            currentChannelWebhook = channel;
            this.LogDebug("Webhook channel override set to: {0}", channel);
            CurrentChannelFeedback.FireUpdate();
        }

        /// <summary>
        /// Resets the webhook channel to the default configured channel
        /// </summary>
        public void ResetChannelWebhook()
        {
            currentChannelWebhook = null;
            this.LogDebug("Webhook channel reset to default: {0}", defaultChannel ?? "(none)");
            CurrentChannelFeedback.FireUpdate();
        }

        #endregion

        #region Bot Methods

        /// <summary>
        /// Gets the current bot channel (override if set, otherwise default)
        /// </summary>
        public string GetCurrentChannelBot()
        {
            return !string.IsNullOrEmpty(currentChannelBot) ? currentChannelBot : defaultChannel ?? string.Empty;
        }

        /// <summary>
        /// Sets the message to be sent on the next bot trigger
        /// </summary>
        /// <param name="message">The message text</param>
        public void SetMessageBot(string message)
        {
            pendingMessageBot = message;
            this.LogDebug("Bot message set: {0}", message);
        }

        /// <summary>
        /// Sends the pending message to Slack via bot
        /// </summary>
        public void SendMessageBot()
        {
            if (string.IsNullOrEmpty(pendingMessageBot))
            {
                this.LogWarning("No bot message to send");
                return;
            }

            SendBotMessageAsync(pendingMessageBot);
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
            currentChannelBot = channel;
            this.LogDebug("Bot channel override set to: {0}", channel);
            CurrentChannelBotFeedback.FireUpdate();
        }

        /// <summary>
        /// Resets the bot channel to the default configured channel
        /// </summary>
        public void ResetChannelBot()
        {
            currentChannelBot = null;
            this.LogDebug("Bot channel reset to default: {0}", defaultChannel ?? "(none)");
            CurrentChannelBotFeedback.FireUpdate();
        }

        #endregion

        #region Webhook Async Methods

        private async void SendWebhookMessageAsync(string message)
        {
            if (!WebhookConfigured)
            {
                this.LogError("Webhook URL is not configured");
                lastSendSuccessfulWebhook = false;
                LastSendSuccessfulFeedback.FireUpdate();
                return;
            }

            if (isBusyWebhook)
            {
                this.LogWarning("Webhook is busy sending a message, please wait");
                return;
            }

            isBusyWebhook = true;
            IsBusyFeedback.FireUpdate();

            try
            {
                var payload = new SlackMessagePayload
                {
                    Text = message,
                    Channel = GetCurrentChannelWebhook(),
                    Username = defaultUsername,
                    IconEmoji = defaultIconEmoji
                };

                var json = JsonConvert.SerializeObject(payload);
                this.LogDebug("Sending Slack message via webhook: {0}", json);
                this.LogVerbose("Webhook request payload: {0}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(webhookUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    this.LogInformation("Message sent successfully via webhook");
                    this.LogVerbose("Webhook success - Payload: {0}, Response: {1}", json, responseBody);
                    lastSendSuccessfulWebhook = true;
                }
                else
                {
                    this.LogError("Failed to send webhook message. Status: {0}, Response: {1}", response.StatusCode, responseBody);
                    this.LogVerbose("Webhook error - Payload: {0}, Status: {1}, Response: {2}", json, response.StatusCode, responseBody);
                    lastSendSuccessfulWebhook = false;
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception sending webhook message: {0}", ex.Message);
                lastSendSuccessfulWebhook = false;
            }
            finally
            {
                isBusyWebhook = false;
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
                lastSendSuccessfulBot = false;
                LastSendSuccessfulBotFeedback.FireUpdate();
                return;
            }

            var channel = GetCurrentChannelBot();
            if (string.IsNullOrEmpty(channel))
            {
                this.LogError("Channel is required when using Bot Token");
                lastSendSuccessfulBot = false;
                LastSendSuccessfulBotFeedback.FireUpdate();
                return;
            }

            if (isBusyBot)
            {
                this.LogWarning("Bot is busy sending a message, please wait");
                return;
            }

            isBusyBot = true;
            IsBusyBotFeedback.FireUpdate();

            try
            {
                var payload = new SlackBotApiPayload
                {
                    Channel = channel,
                    Text = message,
                    Username = defaultUsername,
                    IconEmoji = defaultIconEmoji
                };

                var json = JsonConvert.SerializeObject(payload);
                this.LogDebug("Sending Slack message via Bot API to {0}: {1}", channel, message);
                this.LogVerbose("Bot API request payload: {0}", json);

                var request = new HttpRequestMessage(HttpMethod.Post, slackApiUrl);
                request.Headers.Add("Authorization", "Bearer " + botToken);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<SlackApiResponse>(responseBody);
                    if (apiResponse != null && apiResponse.Ok)
                    {
                        this.LogInformation("Message sent successfully via Bot API");
                        this.LogVerbose("Bot API success - Payload: {0}, Response: {1}", json, responseBody);
                        lastSendSuccessfulBot = true;
                    }
                    else
                    {
                        this.LogError("Slack API error: {0}", apiResponse?.Error ?? "Unknown error");
                        this.LogVerbose("Bot API error - Payload: {0}, Response: {1}", json, responseBody);
                        lastSendSuccessfulBot = false;
                    }
                }
                else
                {
                    this.LogError("Failed to send bot message. Status: {0}, Response: {1}", response.StatusCode, responseBody);
                    this.LogVerbose("Bot API HTTP error - Payload: {0}, Status: {1}, Response: {2}", json, response.StatusCode, responseBody);
                    lastSendSuccessfulBot = false;
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception sending bot message: {0}", ex.Message);
                lastSendSuccessfulBot = false;
            }
            finally
            {
                isBusyBot = false;
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