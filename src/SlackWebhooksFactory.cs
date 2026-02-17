using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Plugins.Slack.Webhooks
{
    public class SlackWebhooksFactory : EssentialsPluginDeviceFactory<SlackWebhooksController>
    {
        public SlackWebhooksFactory()
        {
            MinimumEssentialsFrameworkVersion = "2.24.4";
            TypeNames = new List<string> { "slackWebhooks" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Building Slack Webhooks device: {0}", dc.Key);

            var propertiesConfig = dc.Properties.ToObject<SlackWebhooksPropertiesConfig>();

            if (propertiesConfig == null)
            {
                Debug.LogMessage(LogEventLevel.Error, "Unable to deserialize properties config for device: {0}", dc.Key);
                return null;
            }

            if (string.IsNullOrEmpty(propertiesConfig.WebhookUrl))
            {
                Debug.LogMessage(LogEventLevel.Warning, "No webhook URL configured for device: {0}", dc.Key);
            }

            return new SlackWebhooksController(dc.Key, dc.Name, propertiesConfig);
        }
    }
}

