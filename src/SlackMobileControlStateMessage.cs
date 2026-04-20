using System;
using Newtonsoft.Json;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;

namespace PepperDash.Essentials.Plugins.Slack
{
    public class SlackMobileControlStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("iconEmoji")]
        public string IconEmoji { get; set; }
    }
}