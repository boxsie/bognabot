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
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Bognabot.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Attach Config
            Cfg.AttachConfig(serviceProvider);

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            LogManager.LoadConfiguration($"{AppDomain.CurrentDomain.BaseDirectory}/nlog.config");

            var jobService = serviceProvider.GetService<JobService>();
            Task.Run(() => ConfigureApp(jobService));

            serviceProvider.GetService<App>().Run();

            Console.Read();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
            
            var configuration = Config.Cfg.BuildConfig(services, AppDomain.CurrentDomain.BaseDirectory);

            services.AddTransient<BitmexSocketClient>();
            services.AddTransient<BitmexHttpClient>();
            services.AddSingleton<BitmexService>();

            services.AddTransient<App>();
        }

        private static async Task ConfigureApp(JobService jobService)
        {
            await Cfg.LoadUserDataAsync();
            await jobService.RunAsync();
        }
    }
}
