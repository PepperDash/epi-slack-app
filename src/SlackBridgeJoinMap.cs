using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugins.Slack
{
    /// <summary>
    /// Slack Webhooks Bridge Join Map
    /// </summary>
    public class SlackBridgeJoinMap : JoinMapBaseAdvanced
    {
        #region Digital

        [JoinName("SendMessageWebhook")]
        public JoinDataComplete SendMessageWebhook = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to send the pending webhook message to Slack",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("IsBusyWebhook")]
        public JoinDataComplete IsBusyWebhook = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "High when a webhook message is being sent",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LastSendSuccessfulWebhook")]
        public JoinDataComplete LastSendSuccessfulWebhook = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 3,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "High if the last webhook message send was successful",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ResetChannelWebhook")]
        public JoinDataComplete ResetChannelWebhook = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 4,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to reset webhook channel to default configured channel",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("SendMessageBot")]
        public JoinDataComplete SendMessageBot = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 7,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to send the pending bot message via Bot Token",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("IsBusyBot")]
        public JoinDataComplete IsBusyBot = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 7,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "High when a bot message is being sent",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LastSendSuccessfulBot")]
        public JoinDataComplete LastSendSuccessfulBot = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 8,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "High if the last bot message send was successful",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("ResetChannelBot")]
        public JoinDataComplete ResetChannelBot = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 9,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to reset bot channel to default configured channel",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Analog



        #endregion

        #region Serial

        [JoinName("DeviceName")]
        public JoinDataComplete DeviceName = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Device Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("MessageTextWebhook")]
        public JoinDataComplete MessageTextWebhook = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Set the webhook message text to send (use SendMessageWebhook digital to trigger)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("SendMessageDirectWebhook")]
        public JoinDataComplete SendMessageDirectWebhook = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 3,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Send a webhook message directly (sends immediately when string is received)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("ChannelWebhook")]
        public JoinDataComplete ChannelWebhook = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 4,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Webhook channel override (set to change channel, feedback shows current)",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("MessageTextBot")]
        public JoinDataComplete MessageTextBot = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 7,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Set the bot message text to send (use SendMessageBot digital to trigger)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("SendMessageDirectBot")]
        public JoinDataComplete SendMessageDirectBot = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 8,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Send a message directly via Bot (sends immediately when string is received)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("ChannelBot")]
        public JoinDataComplete ChannelBot = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 9,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Bot channel/user override (set to change target, feedback shows current)",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public SlackBridgeJoinMap(uint joinStart)
            : base(joinStart, typeof(SlackBridgeJoinMap))
        {
        }

    }
}