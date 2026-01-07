using AutoKeySwitch.Core.Services;

namespace AutoKeySwitch.Service
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int checkInterval = configuration.GetValue<int>("AutoKeySwitch:CheckIntervalMs");
            logger.LogInformation("Launch Service - Interval: {CheckInterval}ms", checkInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Detect foreground app
                (string appName, string appPath) = AppMonitor.DetectForegroundApp();
                logger.LogInformation("{AppName} : {AppPath}", appName, appPath);

                await Task.Delay(checkInterval, stoppingToken);
            }
        }
    }
}