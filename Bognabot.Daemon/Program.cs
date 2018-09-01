

using System;
using System.Threading.Tasks;
using Bognabot.Config;
using Bognabot.Exchanges.Bitmex;
using Bognabot.Exchanges.Bitmex.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

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
            Config.App.AttachConfig(AppDomain.CurrentDomain.BaseDirectory, serviceProvider).GetAwaiter().GetResult();

            serviceProvider.GetService<App>().Run();

            Console.Read();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
            
            var configuration = Config.App.BuildConfig(services);

            services.AddSingleton<BitmexStream>();

            services.AddTransient<App>();
        }

        private static IHostingEnvironment GetHostingEnviroment()
        {
            return new HostingEnvironment
            {
                EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ApplicationName = AppDomain.CurrentDomain.FriendlyName,
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
            };
        }
    }
}
