using AutoKeySwitch.Service;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.File(
        path: Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoKeySwitch/Logs",
            "service-.log"
        ),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        flushToDiskInterval: TimeSpan.FromSeconds(10),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("=== AutoKeySwitch Service Starting ===");

    var builder = Host.CreateApplicationBuilder(args);

    // Replace default logger
    builder.Services.AddSerilog();

    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();

    Log.Information("Service configured successfully");

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service crashed during startup");
}
finally
{
    Log.CloseAndFlush();
}

