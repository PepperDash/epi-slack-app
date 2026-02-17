using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugins.Slack.Webhooks
{
    /// <summary>
    /// Slack Webhooks Bridge Join Map
    /// </summary>
    public class SlackWebhooksBridgeJoinMap : JoinMapBaseAdvanced
    {
        #region Digital

        [JoinName("SendMessage")]
        public JoinDataComplete SendMessage = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 1,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Pulse to send the pending message to Slack",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("IsBusy")]
        public JoinDataComplete IsBusy = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "High when a message is being sent",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("LastSendSuccessful")]
        public JoinDataComplete LastSendSuccessful = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 3,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "High if the last message send was successful",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
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

        [JoinName("MessageText")]
        public JoinDataComplete MessageText = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 2,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Set the message text to send (use SendMessage digital to trigger)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("SendMessageDirect")]
        public JoinDataComplete SendMessageDirect = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 3,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Send a message directly (sends immediately when string is received)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public SlackWebhooksBridgeJoinMap(uint joinStart)
            : base(joinStart, typeof(SlackWebhooksBridgeJoinMap))
        {
        }

    }
}