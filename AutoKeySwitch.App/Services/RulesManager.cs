using System.Text.Json;
using AutoKeySwitch.App.Models;

namespace AutoKeySwitch.App.Services
{
    public static class RulesManager
    {
        private static readonly JsonSerializerOptions _writeOptions = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions _readOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Gets the configured keyboard layout for the specified app
        /// </summary>
        /// <param name="appName">App name</param>
        /// <param name="appPath">Full path to app executable</param>
        /// <returns>Keyboard layout identifier</returns>
        public static string GetLayoutForApp(string appName, string appPath)
        {
            try
            {
                RulesConfig config = LoadRules();

                // First pass: Match on full path
                foreach (GameRule rule in config.Rules)
                {
                    if (!string.IsNullOrEmpty(rule.AppPath) &&
                        !string.IsNullOrEmpty(appPath) &&
                        rule.AppPath == appPath &&
                        rule.Enabled)
                    {
                        return rule.Layout;
                    }
                }

                // Second pass: Match on app name
                foreach (GameRule rule in config.Rules)
                {
                    if (!string.IsNullOrEmpty(rule.AppName) &&
                        !string.IsNullOrEmpty(appName) &&
                        rule.AppName == appName &&
                        rule.Enabled)
                    {
                        return rule.Layout;
                    }
                }

                // No match found, return default layout
                return config.DefaultLayout;
            }
            catch
            {
                return "fr-FR";
            }
        }

        /// <summary>
        /// Loads rules from the configuration file
        /// </summary>
        /// <returns>Rules configuration object</returns>
        private static RulesConfig LoadRules()
        {
            try
            {
                EnsureConfigExists();
                string configPath = GetConfigPath();
                string json = File.ReadAllText(configPath);
                RulesConfig? rulesConfig = JsonSerializer.Deserialize<RulesConfig>(json, _readOptions);
                return rulesConfig ?? new RulesConfig();
            }
            catch
            {
                return new RulesConfig();
            }
        }

        /// <summary>
        /// Ensures the configuration file exists, create it with defaults if missing
        /// </summary>
        private static void EnsureConfigExists()
        {
            string configPath = GetConfigPath();
            string? folderPath = Path.GetDirectoryName(configPath);

            // Create directory if it doesn't exist
            if (folderPath != null && !Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create default config file if it doesn't exist
            if (!File.Exists(configPath))
            {
                RulesConfig defaultConfig = new();
                string json = JsonSerializer.Serialize(defaultConfig, _writeOptions);
                File.WriteAllText(configPath, json);
            }
        }

        /// <summary>
        /// Gets the configuration file path
        /// </summary>
        /// <returns>Full path to rules.json in AppData</returns>
        private static string GetConfigPath()
        {
            return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AutoKeySwitch",
                    "rules.json"
                );
        }
    }
}