using System;
using Newtonsoft.Json;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;

namespace PepperDash.Essentials.Plugins.Slack
{
    public class SlackMobileControlBotMessenger : MessengerBase
    {
        private readonly SlackController _controller;

        public SlackMobileControlBotMessenger(string key, string path, SlackController controller)
            : base(key, path, controller)
        {
            _controller = controller;
        }

        public void SendMessage(string message, string channel = null)
        {
            _controller.SendMessageDirectBot(message);
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
                    Channel = _controller.GetCurrentChannelBot(),
                    Text = "Current Slack Controller Status",
                    Username = _controller.DefaultUsername,
                    IconEmoji = _controller.DefaultIconEmoji
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