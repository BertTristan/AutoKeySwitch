using System.IO.Pipes;
using System.Text.Json;
using AutoKeySwitch.Core.Models.Messages;
using AutoKeySwitch.Core.Services;
using Serilog;

namespace AutoKeySwitch.Service
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration, IHostApplicationLifetime hostLifetime) : BackgroundService
    {
        private string _lastAppName = "";
        private string _lastAppPath = "";
        private NamedPipeServerStream? _pipeServer;
        private StreamWriter? _writer;

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            hostLifetime.ApplicationStopping.Register(OnStopping);
            hostLifetime.ApplicationStopped.Register(OnStopped);

            return base.StartAsync(cancellationToken);
        }

        private void OnStopping()
        {
            Log.Information("=== Service Stopping (Lifetime) ===");
            logger.LogInformation("Cleaning up resources...");
        }

        private void OnStopped()
        {
            Log.Information("=== Service Stopped ===");
            Log.CloseAndFlush();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            int checkInterval = configuration.GetValue<int>("AutoKeySwitch:CheckIntervalMs");
            logger.LogInformation("Launch Service - Interval: {CheckInterval}ms", checkInterval);

                try
                {
                    // Establish named pipe connection
                    await CreatePipeConnection(cancellationToken);

                    // Start foreground app monitoring
                    await StartDetectionLoop(checkInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Service stopping...");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Service error");
                }
                finally
                {
                    _writer?.Dispose();
                    _pipeServer?.Dispose();
                }
        }

        /// <summary>
        /// Creates named pipe server and waits for app connection
        /// </summary>
        private async Task CreatePipeConnection(CancellationToken cancellationToken)
        {
            _pipeServer = new NamedPipeServerStream(
                "AutoKeySwitchPipe",
                PipeDirection.Out,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous
            );

            logger.LogInformation("wait App to connect");

            await _pipeServer.WaitForConnectionAsync(cancellationToken);

            logger.LogInformation("App connected");

            _writer = new StreamWriter(_pipeServer)
            {
                AutoFlush = true,
            };
        }

        /// <summary>
        /// Monitors foreground app change and sends layout switch messages
        /// </summary>
        private async Task StartDetectionLoop(int checkInterval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
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

                        await SendSwitchLayout(appName, appPath, layout);

                        _lastAppName = appName;
                        _lastAppPath = appPath;
                    }
                await Task.Delay(checkInterval, cancellationToken);
            }
        }

        /// <summary>
        /// Sends layout switch message by named pipe
        /// </summary>
        private async Task SendSwitchLayout(string appName, string appPath, string layout)
        {
            if (_writer == null)
            {
                logger.LogWarning("Pipe not connected, cannot send message");
                return;
            }

            try
            {
                var msg = new SwitchLayoutMessage
                {
                    AppName = appName,
                    AppPath = appPath,
                    Layout = layout
                };

                string json = JsonSerializer.Serialize(msg);
                await _writer.WriteLineAsync(json);

                logger.LogInformation("Sent: {AppName} > {Layout}", appName, layout);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send message");
            }
        }
    }
}