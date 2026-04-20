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

**Option 1: Using Webhook URL (Simple, but limited to one channel)**

The example below uses the `secretstore` to allow users to add the values using the console commands.

```json
{
	"key": "slack-webhooks-1",
	"uid": 1,
	"name": "Slack Webhooks",
	"type": "slackWebhooks",
	"group": "api",
	"properties": {
		"webhookUrl": {
			"secret": {
				"provider": "default",
				"key": "webhookUrl"
			}
		},
		"defaultUsername": "Crestron System",
		"defaultIconEmoji": ":robot_face:",
		"defaultChannel": "#general"
	}
}
```

**Option 2: Using Bot Token (Flexible, can send to any channel or DM users)**

The example below uses the `secretstore` to allow users to add the values using the console commands.

```json
{
	"key": "slack-webhooks-1",
	"uid": 1,
	"name": "Slack Webhooks",
	"type": "slackWebhooks",
	"group": "api",
	"properties": {
		"botToken": {
			"secret": {
				"provider": "default",
				"key": "botToken"
			}
		},
		"defaultUsername": "Crestron System",
		"defaultIconEmoji": ":robot_face:",
		"defaultChannel": "#general"
	}
}
```

### Secret Store

**Set the secrets**
```c#
setsecret:1 default webhookUrl {SLACK-WEBHOOK-URL}
setsecret:1 default botToken {SLACK-BOT-TOKEN}
```

**Update the secrets**
```c#
updatesecret:1 default webhookUrl {UPDATED-WEBHOOK-URL}
updatesecret:1 default botToken {UPDATED-BOT-TOKEN}
```

### Properties

| Property           | Type   | Required               | Description                                                     |
| ------------------ | ------ | ---------------------- | --------------------------------------------------------------- |
| `webhookUrl`       | string | Yes (if no botToken)   | The Slack Incoming Webhook URL                                  |
| `botToken`         | string | Yes (if no webhookUrl) | Slack Bot Token (starts with `xoxb-`). Allows DMs & any channel |
| `defaultUsername`  | string | No                     | Override the default username for messages                      |
| `defaultIconEmoji` | string | No                     | Override the default icon (e.g., `:robot_face:`)                |
| `defaultChannel`   | string | No (Yes for Bot Token) | Default channel or user to send messages to                     |
| `customPayloadTemplate` | string | No                | Custom JSON payload template for non-standard webhook endpoints |

## Custom Payload Template

The `customPayloadTemplate` property allows you to send custom JSON payloads to non-Slack webhook endpoints. This is useful when integrating with third-party systems that expect a specific JSON structure.

### Message Format

When using a custom payload template, messages sent from SIMPL should follow this format:

```
Suite {number} - {request_type}
```

Or with an additional message:

```
Suite {number} - {request_type} - {additional message}
```

### Available Tokens

| Token | Description |
|-------|-------------|
| `{{suiteNumber}}` | Extracted suite number from the message |
| `{{requestType}}` | Extracted request type from the message |
| `{{message}}` | Additional text after the request type (if any) |
| `{{rawMessage}}` | The original unparsed message |
| `{{channel}}` | Value from `defaultChannel` config |
| `{{username}}` | Value from `defaultUsername` config |
| `{{iconEmoji}}` | Value from `defaultIconEmoji` config |

### Example Configuration

```json
{
    "key": "slack-app-1",
    "name": "Slack App",
    "type": "slackWebhooks",
    "group": "api",
    "properties": {
        "webhookUrl": {
            "secret": {
                "provider": "default",
                "key": "slack_webhook_url"
            }
        },
        "customPayloadTemplate": "{\"suite_number\": \"{{suiteNumber}}\", \"request_type\": \"{{requestType}}\", \"message\": \"{{message}}\"}"
    }
}
```

### Parsing Examples

| SIMPL Message | suiteNumber | requestType | message |
|---------------|-------------|-------------|---------|
| `Suite 3 - coffee` | `3` | `coffee` | `` |
| `Suite 1 - assistance` | `1` | `assistance` | `` |
| `Suite 5 - help - Need towels` | `5` | `help` | `Need towels` |

### Resulting Webhook Payload

When sending `Suite 3 - coffee` with the example configuration above:

```json
{
    "suite_number": "3",
    "request_type": "coffee",
    "message": ""
}
```

> **Note:** When `customPayloadTemplate` is not configured, the standard Slack webhook payload format is used.

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

| Join | Direction  | Description                                   |
| ---- | ---------- | --------------------------------------------- |
| 2    | From SIMPL | Pulse to send the pending webhook message     |
| 2    | To SIMPL   | High when webhook is sending (busy indicator) |
| 3    | To SIMPL   | High if last webhook send was successful      |
| 4    | From SIMPL | Pulse to reset webhook channel to default     |
| 7    | From SIMPL | Pulse to send the pending bot message         |
| 7    | To SIMPL   | High when bot is sending (busy indicator)     |
| 8    | To SIMPL   | High if last bot send was successful          |
| 9    | From SIMPL | Pulse to reset bot channel to default         |

### Serial Joins

| Join | Direction     | Description                                                     |
| ---- | ------------- | --------------------------------------------------------------- |
| 1    | To SIMPL      | Device name                                                     |
| 2    | From SIMPL    | Set webhook message text (use digital join 2 to trigger send)   |
| 3    | From SIMPL    | Send webhook message directly (sends immediately when received) |
| 4    | To/From SIMPL | Webhook channel override (feedback shows current)               |
| 7    | From SIMPL    | Set bot message text (use digital join 7 to trigger send)       |
| 8    | From SIMPL    | Send bot message directly (sends immediately when received)     |
| 9    | To/From SIMPL | Bot channel/user override (feedback shows current)              |

## Usage

### Webhook Method (Joins 2-4)

Use these joins when you have a webhook URL configured:

1. **Two-step approach**: Set message text on serial join 2, then pulse digital join 2 to send
2. **Direct send**: Send a string on serial join 3 - sends immediately
3. **Channel override**: Set channel on serial join 4 (note: may not work with newer Slack webhooks)

### Bot Method (Joins 7-9)

Use these joins when you have a bot token configured:

1. **Two-step approach**: Set message text on serial join 7, then pulse digital join 7 to send
2. **Direct send**: Send a string on serial join 8 - sends immediately
3. **Dynamic routing**: Set channel/user on serial join 9 (e.g., `#general`, `@username`, or `U0123456789`)


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

## Creating Slack Bot (For Bot Token)

Bot Tokens allow you to send messages to any channel or direct message users. Follow these steps:

### Step 1: Create a Slack App

1. Go to the [Slack API Apps page](https://api.slack.com/apps)
2. Click **Create New App**
3. Select **From scratch**
4. Enter an **App Name** (e.g., "Crestron Bot")
5. Select the **Workspace** where you want to install the app
6. Click **Create App**

### Step 2: Add Bot Scopes

1. In your app's settings, navigate to **Features** > **OAuth & Permissions**
2. Scroll to **Scopes** > **Bot Token Scopes**
3. Click **Add an OAuth Scope** and add these scopes:
   - `chat:write` - Required to send messages
   - `chat:write.public` - Required to send to channels the bot hasn't joined
   - `users:read` - Optional, for looking up user IDs

### Step 3: Install App to Workspace

1. Scroll up to **OAuth Tokens for Your Workspace**
2. Click **Install to Workspace**
3. Review the permissions and click **Allow**
4. Copy the **Bot User OAuth Token** (starts with `xoxb-`)

### Step 4: Configure the Plugin

Use the Bot Token in your Essentials configuration:

```json
{
	"properties": {
		"botToken": "<YOUR_BOT_TOKEN>",
		"defaultChannel": "#general"
	}
}
```

### Sending to Channels vs Users

With a Bot Token, you can set the channel dynamically:

| Target          | Format                 | Example                               |
| --------------- | ---------------------- | ------------------------------------- |
| Public channel  | `#channel-name`        | `#general`                            |
| Private channel | `#channel-name`        | `#private-room` (bot must be invited) |
| Direct message  | `@username` or User ID | `@john.smith` or `U0123456789`        |

**Note:** To DM a user, either:
- Use their User ID (most reliable): `U0123456789`
- The bot must have been added to a conversation with them first

### Finding a User ID

1. In Slack, click on the user's profile
2. Click the **...** (More) button
3. Select **Copy member ID**

### Bot Token Security Notes

- **Keep your bot token secret** - it grants access to send messages as your bot
- Bot tokens do not expire but can be revoked from the Slack App settings
- Consider using different apps/tokens for different environments

