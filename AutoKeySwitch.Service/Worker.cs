using AutoKeySwitch.Core.Services;

namespace AutoKeySwitch.Service
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        private string _lastAppName = "";
        private string _lastAppPath = "";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int checkInterval = configuration.GetValue<int>("AutoKeySwitch:CheckIntervalMs");
            logger.LogInformation("Launch Service - Interval: {CheckInterval}ms", checkInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Detect foreground app
                (string appName, string appPath) = AppMonitor.DetectForegroundApp();

                // Only switch layout if app has changed
                if (!string.IsNullOrEmpty(appName) &&
                    (appName != _lastAppName ||
                    (appPath != _lastAppPath && !string.IsNullOrEmpty(appPath))))
                {
                    string layout = RulesManager.GetLayoutForApp(appName, appPath);

                    logger.LogInformation("App: {AppName} > Layout: {Layout}", appName, layout);

                    _lastAppName = appName;
                    _lastAppPath = appPath;

                }

                await Task.Delay(checkInterval, stoppingToken);
            }
        }
    }
}