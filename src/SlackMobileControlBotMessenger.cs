using System;
using Newtonsoft.Json;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;

namespace PepperDash.Essentials.Plugins.Slack
{
    public class SlackMobileControlBotMessenger : MessengerBase
    {
        private readonly SlackController controller;

        public SlackMobileControlBotMessenger(string key, string path, SlackController controller)
            : base(key, path, controller)
        {
            this.controller = controller;
        }

        public void SendMessage(string message, string channel = null)
        {
            var targetChannel = controller.GetCurrentChannelBot();

            if (!string.IsNullOrEmpty(channel) && channel != targetChannel)
            {
                this.LogWarning("SlackMobileControlBotMessenger: Requested channel '{0}' ignored; bot sends to configured channel '{1}'", channel, targetChannel);
            }

            this.LogInformation("SlackMobileControlBotMessenger: Sending message to channel {0}", targetChannel);
            controller.SendMessageDirectBot(message);
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
                    Channel = controller.GetCurrentChannelBot(),
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