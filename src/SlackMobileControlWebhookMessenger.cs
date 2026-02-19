using System;
using Newtonsoft.Json;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;

namespace PepperDash.Essentials.Plugins.Slack
{
    public class SlackMobileControlWebhookMessenger : MessengerBase
    {
        private readonly SlackController controller;

        public SlackMobileControlWebhookMessenger(string key, string path, SlackController controller)
            : base(key, path, controller)
        {
            this.controller = controller;
        }

        public void SendMessage(string message, string channel = null)
        {
            if (string.IsNullOrEmpty(channel))
            {
                channel = controller.GetCurrentChannelWebhook();
            }
            this.LogInformation("SlackMobileControlWebhookMessenger: Sending message to channel {0}", channel);

            controller.SendMessageDirectWebhook(message);
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var status = new SlackMobileControlStateMessage
                {
                    Channel = controller.GetCurrentChannelWebhook(),
                    Text = "Current Slack Controller Status",
                    Username = controller.defaultUsername,
                    IconEmoji = controller.defaultIconEmoji
                };

                PostStatusMessage(id, status);
            }
            catch (Exception ex)
            {
                this.LogError("Error sending full status: {0}", ex.Message);
            }
        }
    }
}