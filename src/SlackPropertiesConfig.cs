using Newtonsoft.Json;

namespace PepperDash.Essentials.Plugins.Slack
{
    public class SlackPropertiesConfig
    {
        /// <summary>
        /// The Slack Incoming Webhook URL (use this OR botToken, not both)
        /// </summary>
        [JsonProperty("webhookUrl")]
        public string WebhookUrl { get; set; }

        /// <summary>
        /// The Slack Bot Token (starts with xoxb-). Use this to send to any channel or DM users.
        /// Requires chat:write scope. Use this OR webhookUrl, not both.
        /// </summary>
        [JsonProperty("botToken")]
        public string BotToken { get; set; }

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

        public SlackPropertiesConfig()
        {
        }
    }
}