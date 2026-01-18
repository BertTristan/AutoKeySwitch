# AutoKeySwitch

**Windows-only** - Automatically switches keyboard layouts based on the active application.

## Features

- Automatic layout switching when you change apps
- Rule-based configuration via JSON
- Background service
- Full activity logging

## Requirements

- .NET 10 SDK
- Windows

## Quick Start
```bash
# Clone
git clone https://github.com/BertTristan/AutoKeySwitch.git
cd AutoKeySwitch

# Build
dotnet build

# Run Service (first)
cd AutoKeySwitch.Service
dotnet run

# Run App (second, in another terminal)
cd AutoKeySwitch.App
dotnet run
```

> **Note:** Both must be running. Service detects apps, App switches layouts.

## Configuration

Edit `%AppData%\Roaming\AutoKeySwitch\rules.json`:
```json
{
  "DefaultLayout": "fr-FR",
  "Rules": [
    {
      "AppName": "Discord.exe",
      "AppPath": "C:\\Path\\To\\Discord.exe",
      "Layout": "en-US",
      "Enabled": true
    }
  ]
}
```

**Fields:**
- `AppName` - Executable name (e.g., `Discord.exe`)
- `AppPath` - Full path (optional but recommended, takes priority over AppName)
- `Layout` - Target layout (`fr-FR`, `en-US`, `en-GB`)
- `Enabled` - Toggle rule on/off

**Supported layouts:** `fr-FR`, `en-US`, `en-GB`

> **Tip:** To find AppPath, open Task Manager > Details > Right-click app > Open file location

## Logs

Logs are stored in `%AppData%\Roaming\AutoKeySwitch\Logs\`:
- `service-YYYYMMDD.log` - Service activity
- `app-YYYYMMDD.log` - App activity

Logs rotate daily (7 days retention).

## Troubleshooting

**Layout doesn't change:**
- Check that both Service and App are running
- Check logs in `%AppData%\Roaming\AutoKeySwitch\Logs\`

**Service won't start:**
- Ensure .NET 10 SDK is installed
- Check `service-*.log` for errors

## Project Structure
```
AutoKeySwitch.Service/  # Detects active app
AutoKeySwitch.App/      # Switches keyboard layout
AutoKeySwitch.Core/     # Shared logic
```

## License

MIT