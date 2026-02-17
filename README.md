![PepperDash Essentials Plugin Logo](/images/essentials-plugin-blue.png)

# Slack Webhooks Plugin (c) 2026

### Overview

This is a **PepperDash Essentials Plugin** for Slack Webhooks integration. It allows Crestron control systems to send messages to Slack channels via Slack Incoming Webhooks.

### Minimum Essentials Framework Versions

- 2.24.4

## License

Provided under MIT license

## Setting Up Slack Webhooks

1. Go to [Slack API Apps](https://api.slack.com/apps)
2. Create a new app or select an existing one
3. Navigate to "Incoming Webhooks" and activate it
4. Add a new webhook to the desired workspace/channel
5. Copy the webhook URL for use in the configuration

## Configuration Object

### Device

Type: `slackWebhooks`

```json
{
	"key": "slack-webhooks-1",
	"uid": 1,
	"name": "Slack Webhooks",
	"type": "slackWebhooks",
	"group": "api",
	"properties": {
		"webhookUrl": "<YOUR_SLACK_WEBHOOK_URL>",
		"defaultUsername": "Crestron System",
		"defaultIconEmoji": ":robot_face:",
		"defaultChannel": "#general"
	}
}
```

### Properties

| Property           | Type   | Required | Description                                                      |
| ------------------ | ------ | -------- | ---------------------------------------------------------------- |
| `webhookUrl`       | string | Yes      | The Slack Incoming Webhook URL                                   |
| `defaultUsername`  | string | No       | Override the default username for messages                       |
| `defaultIconEmoji` | string | No       | Override the default icon (e.g., `:robot_face:`)                 |
| `defaultChannel`   | string | No       | Override the default channel (requires additional webhook scope) |

### Bridge

```json
{
	"key": "devices-bridge",
	"uid": 11,
	"name": "Devices Bridge",
	"group": "api",
	"type": "eiscApiAdvanced",
	"properties": {
		"control": { "ipid": "a6", "method": "ipidTcp", "tcpSshProperties": { "address": "127.0.0.2", "port": 0 } },
		"devices": [
			{ "deviceKey": "slack-webhooks-1", "joinStart": 1 }
		]
	}
}
```

## Join Map

### Digital Joins

| Join | Direction  | Description                                        |
| ---- | ---------- | -------------------------------------------------- |
| 1    | From SIMPL | Pulse to send the pending message to Slack         |
| 2    | To SIMPL   | High when a message is being sent (busy indicator) |
| 3    | To SIMPL   | High if the last message send was successful       |

### Serial Joins

| Join | Direction  | Description                                                         |
| ---- | ---------- | ------------------------------------------------------------------- |
| 1    | To SIMPL   | Device name                                                         |
| 2    | From SIMPL | Set the message text to send (use digital join 1 to trigger send)   |
| 3    | From SIMPL | Send a message directly (sends immediately when string is received) |

## Usage

There are two ways to send messages:

1. **Two-step approach**: Set the message text on serial join 2, then pulse digital join 1 to send
2. **Direct send**: Send a string on serial join 3 - the message is sent immediately


## Creating Slack App

Follow these steps to create a Slack App and generate an Incoming Webhook URL:

### Step 1: Create a Slack App

1. Go to the [Slack API Apps page](https://api.slack.com/apps)
2. Click **Create New App**
3. Select **From scratch**
4. Enter an **App Name** (e.g., "Crestron Notifications")
5. Select the **Workspace** where you want to install the app
6. Click **Create App**

### Step 2: Enable Incoming Webhooks

1. In your app's settings, navigate to **Features** > **Incoming Webhooks** in the left sidebar
2. Toggle the switch to **On** to activate Incoming Webhooks
3. Scroll down to the **Webhook URLs for Your Workspace** section

### Step 3: Add a Webhook to Your Workspace

1. Click **Add New Webhook to Workspace**
2. Select the **channel** where you want messages to be posted
3. Click **Allow** to authorize the webhook
4. You will be redirected back to the Incoming Webhooks page

### Step 4: Copy the Webhook URL

1. In the **Webhook URLs for Your Workspace** section, you will see your new webhook
2. Click **Copy** to copy the webhook URL
3. The URL will look like: `https://hooks.slack.com/services/TXXXXX/BXXXXX/XXXXXXXXXX`

### Step 5: Configure the Plugin

Use the copied webhook URL in your Essentials configuration:

```json
{
	"properties": {
		"webhookUrl": "<YOUR_SLACK_WEBHOOK_URL>"
	}
}
```

### Optional: Customize Your App

You can customize how your messages appear in Slack:

1. Navigate to **Settings** > **Basic Information** in your app settings
2. Scroll to **Display Information**
3. Add an **App icon** and **description**
4. These will be the default appearance for messages (can be overridden via plugin config)

### Security Notes

- **Keep your webhook URL secret** - anyone with the URL can post messages to your channel
- Webhook URLs do not expire but can be revoked from the Slack App settings
- Consider creating separate webhooks for different environments (dev/prod)

