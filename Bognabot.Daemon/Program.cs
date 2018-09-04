using System;
using System.Threading.Tasks;
using Bognabot.Bitmex;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http;
using Bognabot.Bitmex.Socket;
using Bognabot.Config;
using Bognabot.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Bognabot.Daemon
{
    class Program
    {
        private static Logger _logger;
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
            
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

            Task.Run(ConfigureApp);

            _serviceProvider.GetService<App>().Run();

            Console.Read();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));

            _logger = NLogBuilder.ConfigureNLog($"{AppDomain.CurrentDomain.BaseDirectory}/nlog.config").GetCurrentClassLogger();

            services.AddTransient<BitmexSocketClient>();
            services.AddTransient<BitmexHttpClient>();
            services.AddSingleton<BitmexService>();

            services.AddTransient<App>();
        }

        private static async Task ConfigureApp()
        {
            await Cfg.LoadUserDataAsync(_serviceProvider, AppDomain.CurrentDomain.BaseDirectory);

            await _serviceProvider.GetService<JobService>().RunAsync();
        }
    }
}
