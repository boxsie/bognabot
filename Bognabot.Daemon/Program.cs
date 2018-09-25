using System;
using System.Threading.Tasks;
using Bognabot.Bitmex;
using Bognabot.Core;
using Bognabot.Data;
using Bognabot.Services;
using Bognabot.Services.Exchange;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using ILogger = NLog.ILogger;

namespace Bognabot.Daemon
{
    class Program
    {
        private static ILogger _logger;
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
            
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

            ConfigureApp(_serviceProvider);

            _serviceProvider.GetService<App>().Run();

            Console.Read();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            _logger = NLogBuilder.ConfigureNLog($"{AppDomain.CurrentDomain.BaseDirectory}/nlog.config").GetCurrentClassLogger();
            
            AppInitialise.AddServices(services);

            services.AddTransient<App>();
        }

        private static void ConfigureApp(IServiceProvider serviceProvider)
        {
            AppInitialise.LoadUserData(serviceProvider, AppDomain.CurrentDomain.BaseDirectory);
            
            var sm = serviceProvider.GetService<ServiceManager>();

            Task.Run(() => AppInitialise.Start(sm));
        }
    }
}
