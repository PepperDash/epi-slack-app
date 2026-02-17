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

namespace PepperDash.Essentials.Plugins.Slack.Webhooks
{
    public class SlackWebhooksController : EssentialsBridgeableDevice
    {
        private readonly SlackWebhooksPropertiesConfig _config;
        private readonly HttpClient _httpClient;
        private string _pendingMessage;
        private bool _isBusy;

        /// <summary>
        /// Feedback indicating if the device is currently sending a message
        /// </summary>
        public BoolFeedback IsBusyFeedback { get; private set; }

        /// <summary>
        /// Feedback indicating the last message send was successful
        /// </summary>
        public BoolFeedback LastSendSuccessfulFeedback { get; private set; }

        private bool _lastSendSuccessful;

        public SlackWebhooksController(string key, string name, SlackWebhooksPropertiesConfig propertiesConfig)
            : base(key, name)
        {
            _config = propertiesConfig;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            IsBusyFeedback = new BoolFeedback(key + "-IsBusy", () => _isBusy);
            LastSendSuccessfulFeedback = new BoolFeedback(key + "-LastSendSuccessful", () => _lastSendSuccessful);
        }

        public override void Initialize()
        {
            base.Initialize();

            if (string.IsNullOrEmpty(_config.WebhookUrl))
            {
                this.LogWarning("Webhook URL is not configured");
            }
            else
            {
                this.LogDebug("Slack Webhook configured for: {0}", _config.WebhookUrl.Substring(0, Math.Min(50, _config.WebhookUrl.Length)) + "...");
            }
        }

        /// <summary>
        /// Sets the message to be sent on the next trigger
        /// </summary>
        /// <param name="message">The message text</param>
        public void SetMessage(string message)
        {
            _pendingMessage = message;
            this.LogDebug("Message set: {0}", message);
        }

        /// <summary>
        /// Sends the pending message to Slack
        /// </summary>
        public void SendMessage()
        {
            if (string.IsNullOrEmpty(_pendingMessage))
            {
                this.LogWarning("No message to send");
                return;
            }

            SendMessageAsync(_pendingMessage);
        }

        /// <summary>
        /// Sends a message directly to Slack without setting it first
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessageDirect(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                this.LogWarning("Cannot send empty message");
                return;
            }

            SendMessageAsync(message);
        }

        private async void SendMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(_config.WebhookUrl))
            {
                this.LogError("Webhook URL is not configured");
                _lastSendSuccessful = false;
                LastSendSuccessfulFeedback.FireUpdate();
                return;
            }

            if (_isBusy)
            {
                this.LogWarning("Already sending a message, please wait");
                return;
            }

            _isBusy = true;
            IsBusyFeedback.FireUpdate();

            try
            {
                var payload = new SlackMessagePayload
                {
                    Text = message,
                    Channel = _config.DefaultChannel,
                    Username = _config.DefaultUsername,
                    IconEmoji = _config.DefaultIconEmoji
                };

                var json = JsonConvert.SerializeObject(payload);

                this.LogDebug("Sending Slack message: {0}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_config.WebhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    this.LogInformation("Message sent successfully");
                    _lastSendSuccessful = true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    this.LogError("Failed to send message. Status: {0}, Response: {1}", response.StatusCode, responseBody);
                    _lastSendSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                this.LogError("Exception sending Slack message: {0}", ex.Message);
                _lastSendSuccessful = false;
            }
            finally
            {
                _isBusy = false;
                IsBusyFeedback.FireUpdate();
                LastSendSuccessfulFeedback.FireUpdate();
            }
        }

        #region IBridgeAdvanced Members

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new SlackWebhooksBridgeJoinMap(joinStart);

            // This adds the join map to the collection on the bridge
            bridge?.AddJoinMap(Key, joinMap);

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            this.LogDebug("Linking to Trilist {id}", trilist.ID.ToString("X"));
            this.LogInformation("Linking to Bridge Type {type}", GetType().Name);

            // Digital joins
            trilist.SetSigTrueAction(joinMap.SendMessage.JoinNumber, SendMessage);

            IsBusyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsBusy.JoinNumber]);
            LastSendSuccessfulFeedback.LinkInputSig(trilist.BooleanInput[joinMap.LastSendSuccessful.JoinNumber]);

            // Serial joins
            trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
            trilist.SetStringSigAction(joinMap.MessageText.JoinNumber, SetMessage);
            trilist.SetStringSigAction(joinMap.SendMessageDirect.JoinNumber, SendMessageDirect);

            trilist.OnlineStatusChange += (sender, args) =>
            {
                if (!args.DeviceOnLine) return;

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
                IsBusyFeedback.FireUpdate();
                LastSendSuccessfulFeedback.FireUpdate();
            };

            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine) return;

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
            };
        }

        #endregion
    }

    /// <summary>
    /// Slack message payload structure
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
}