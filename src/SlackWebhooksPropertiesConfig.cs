using Newtonsoft.Json;

namespace PepperDash.Essentials.Plugins.Slack.Webhooks
{
    public class SlackWebhooksPropertiesConfig
    {
        /// <summary>
        /// The Slack Incoming Webhook URL
        /// </summary>
        [JsonProperty("webhookUrl")]
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Optional default channel override (requires webhook scope)
        /// </summary>
        [JsonProperty("defaultChannel")]
        public string DefaultChannel { get; set; }

        /// <summary>
        /// Optional default username override
        /// </summary>
        [JsonProperty("defaultUsername")]
        public string DefaultUsername { get; set; }

        /// <summary>
        /// Optional default icon emoji (e.g., ":robot_face:")
        /// </summary>
        [JsonProperty("defaultIconEmoji")]
        public string DefaultIconEmoji { get; set; }

        public SlackWebhooksPropertiesConfig()
        {
        }
    }
}