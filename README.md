# AutoKeySwitch

**Windows-only** - Automatically switches keyboard layouts based on the active application.

## Features

- Automatic layout switching when you change apps
- Rule-based configuration via JSON
- Hot-reload: rules.json changes are applied instantly without restarting
- Background monitoring
- Full activity logging

## Requirements

- .NET 10 SDK
- Windows 11 (maybe Windows 10)

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
- Verify AppName and AppPath match process name (Task Manager > Details)

**Known limitations:**
- Elevated applications (e.g., Task Manager) cannot be switched due to Windows security restrictions (UIPI). This affects all similar tools.

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