using Serilog;
using Serilog.Core;
using System.Configuration;
using System.Data;
using System.Windows;

namespace OpenCVwpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILogger _logger;
        protected override void OnStartup(StartupEventArgs e)
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}")
                .WriteTo.File("logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{Operation}] {Message:lj}{NewLine}{Properties:j}{NewLine}{Exception}")
                .Enrich.FromLogContext()  // This line is important for context properties
                .CreateLogger();
            Log.Logger = _logger;
            try
            {
                Log.Information("Application Starting Up");
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                throw;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application Shutting Down");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }

   

}
