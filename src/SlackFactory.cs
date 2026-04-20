using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Plugins.Slack
{
    public class SlackFactory : EssentialsPluginDeviceFactory<SlackController>
    {
        public SlackFactory()
        {
            MinimumEssentialsFrameworkVersion = "2.24.4";
            TypeNames = new List<string> { "slackWebhooks", "slackbot", "slackapp" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Building Slack device: {0}", dc.Key);

            var propertiesConfig = dc.Properties.ToObject<SlackPropertiesConfig>();

            if (propertiesConfig == null)
            {
                Debug.LogMessage(LogEventLevel.Error, "Unable to deserialize properties config for device: {0}", dc.Key);
                return null;
            }

            if (string.IsNullOrEmpty(propertiesConfig.WebhookUrl) && string.IsNullOrEmpty(propertiesConfig.BotToken))
            {
                Debug.LogMessage(LogEventLevel.Warning, "No webhook URL or bot token configured for device: {0}", dc.Key);
            }

            if (!string.IsNullOrEmpty(propertiesConfig.WebhookUrl) && !string.IsNullOrEmpty(propertiesConfig.BotToken))
            {
                Debug.LogMessage(LogEventLevel.Warning, "Both webhook URL and bot token configured for device: {0}. Consider using only one.", dc.Key);
            }

            return new SlackController(dc.Key, dc.Name, propertiesConfig);
        }
    }
}

