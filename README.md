# AutoKeySwitch

**Windows-only** - Automatically switches keyboard layouts based on the active application.

## Features

- Automatic layout switching when you change apps
- Rule-based configuration via JSON
- Background monitoring
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

# Run App
cd AutoKeySwitch.App
dotnet run
```

## Configuration

Edit `%AppData%\Roaming\AutoKeySwitch\rules.json`:
```json
{
  "DefaultLayout": "fr-FR",
  "Rules": [
    {
      "AppName": "Discord",
      "AppPath": "C:\\Path\\To\\Discord.exe",
      "Layout": "en-US",
      "Enabled": true
    }
  ]
}
```

**Fields:**
- `AppName` - Process name without .exe (e.g., `Discord`)
- `AppPath` - Full path (optional but recommended, takes priority over AppName)
- `Layout` - Target layout (`fr-FR`, `en-US`, `en-GB`)
- `Enabled` - Toggle rule on/off

**Supported layouts:** `fr-FR`, `en-US`, `en-GB`

> **Tip:** To find AppPath, open Task Manager > Details > Right-click app > Open file location

## Logs

Logs are stored in `%AppData%\Roaming\AutoKeySwitch\Logs\`:
- `aks-YYYYMMDD.log` - App activity

Logs rotate daily (7 days retention).

## Troubleshooting

**Layout doesn't change:**
- Ensure the App is running
- Check logs in `%AppData%\Roaming\AutoKeySwitch\Logs\`

## Project Structure
```
AutoKeySwitch/
└── AutoKeySwitch.App/
    ├── Models/             # GameRule, RulesConfig
    ├── Services/           # AppMonitor, RulesManager, LayoutSwitcher
    └── App.xaml.cs         # Main logic
```

## License

MIT