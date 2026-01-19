using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using AutoKeySwitch.Core.Models.Messages;
using AutoKeySwitch.App.Services;
using System;
using Serilog;

namespace AutoKeySwitch.App
{

    public partial class App : Application
    {
        private NamedPipeClientStream? _pipeClient;
        private CancellationTokenSource? _cancellationTokenSource;

        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "AutoKeySwitch/Logs",
                        "app-.log"
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
            StartPipeListener(_cancellationTokenSource.Token);
        }


        /// <summary>
        /// Starts pipe listener and message processing loop
        /// </summary>
        private async void StartPipeListener(CancellationToken cancellationToken)
        {
            try
            {
                // Connect to service pipe
                StreamReader? reader = await ConnectToService(cancellationToken);

                if (reader is not null)
                {
                    await ReadMessage(reader, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pipe listener error");
            }
            finally
            {
                _pipeClient?.Dispose();
                _cancellationTokenSource?.Dispose();

                Log.Information("=== App Stopping ===");
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Connects to service named pipe
        /// </summary>
        /// <returns>StreamReader for reading messages, or null if connection failed</returns>
        private async Task<StreamReader?> ConnectToService(CancellationToken cancellationToken)
        {
            try
            {
                Log.Information("Connecting to Service pipe...");

                _pipeClient = new NamedPipeClientStream(".", "AutoKeySwitchPipe", PipeDirection.In);
                await _pipeClient.ConnectAsync(cancellationToken);

                Log.Information("Connected to Service");

                return new StreamReader(_pipeClient);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Connection failed");
                _pipeClient?.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Reads messages from pipe continuously
        /// </summary>
        private async Task ReadMessage(StreamReader reader, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string? message = await reader.ReadLineAsync(cancellationToken);

                if (message == null) break;

                if (!string.IsNullOrEmpty(message))
                {
                    ProcessMessage(message);
                }
            }
        }

        /// <summary>
        /// Processes received messages and dispatches to appropriate handlers
        /// </summary>
        private void ProcessMessage(string message)
        {
            try
            {
                // Parse message type
                using JsonDocument msgContent = JsonDocument.Parse(message);
                string? msgType = msgContent.RootElement.GetProperty("Type").GetString();

                if (string.IsNullOrEmpty(msgType))
                    return;

                switch (msgType)
                {
                    case "SwitchLayout":
                        var switchMsg = JsonSerializer.Deserialize<SwitchLayoutMessage>(message);
                        if (!string.IsNullOrEmpty(switchMsg?.Layout))
                        {
                            Log.Information("Received: {Layout}", switchMsg.Layout);
                            LayoutSwitcher.ChangeLayout(switchMsg.Layout);
                        }
                        break;

                    default:
                        Log.Warning("Unknown message type: {MessageType}", msgType);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Message processing error");
            }
        }
    }
}