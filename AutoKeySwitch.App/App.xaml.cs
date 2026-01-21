using AutoKeySwitch.App.Services;
using Microsoft.UI.Xaml;
using Serilog;

namespace AutoKeySwitch.App
{

    public partial class App : Application
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private string _lastAppName = "";
        private string _lastAppPath = "";
        private string _lastLayout = "";

        public App()
        {
            // Create Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "AutoKeySwitch/Logs",
                        "aks-.log"
                    ),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    flushToDiskInterval: TimeSpan.FromSeconds(10),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            Log.Information("=== AutoKeySwitch App Starting ===");

            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Log.Information("App launched");

            _cancellationTokenSource = new CancellationTokenSource();

            RunDetectionLoop(_cancellationTokenSource.Token);
        }


        /// <summary>
        /// The main loop for swap layout
        /// </summary>
        /// <param name="cancellationToken">A token for cancel</param>
        private async void RunDetectionLoop(CancellationToken cancellationToken)
        {
            try
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
                        // Retrieve layout to apply
                        string layout = RulesManager.GetLayoutForApp(appName, appPath);

                        if (layout != _lastLayout)
                        {
                            Log.Information("App: {AppName} → Layout: {Layout}", appName, layout);

                            LayoutSwitcher.ChangeLayout(layout);

                            _lastLayout = layout;
                        }
                       
                        // Update last app
                        _lastAppName = appName;
                        _lastAppPath = appPath;
                    }
                    await Task.Delay(500, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Information("Detection loop cancelled");
            }
            catch (Exception ex) 
            {
                Log.Error(ex, "Detection loop error");
            }
            finally
            {
                Log.Information("=== App Stopping ===");
                Log.CloseAndFlush();
            }
        }
    }
}