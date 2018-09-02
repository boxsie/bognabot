using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.App.Hubs;
using Bognabot.Bitmex;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http;
using Bognabot.Bitmex.Socket;
using Bognabot.Config;
using Bognabot.Data;
using Bognabot.Data.Core;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Mapping;
using Bognabot.Data.Models.Exchange;
using Bognabot.Data.Repository;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Jobs;
using Bognabot.Jobs.Init;
using Bognabot.Jobs.Sync;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Bognabot.App
{
    public class Startup
    {
        private IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var logger = NLogBuilder.ConfigureNLog($"{_env.ContentRootPath}/nlog.config").GetCurrentClassLogger();

            var config = Cfg.BuildConfig(services, _env.ContentRootPath);

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            services.AddSignalR();

            services.AddTransient<BitmexSocketClient>();
            services.AddTransient<BitmexHttpClient>();
            services.AddSingleton<IExchangeService, BitmexService>();

            services.AddSingleton<RepositoryService>();
            services.AddTransient<IRepository<Candle>, Repository<Candle>>();

            services.AddSingleton<JobService>();
            services.AddTransient<CandleSync>();
            services.AddTransient<CandleCatchup>();
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseWebSockets();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<StreamHub>("/streamhub");
            });

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<DataProfile>();
                cfg.AddProfile<BitmexProfile>();
            });

            Cfg.AttachConfig(serviceProvider);

            var jobService = serviceProvider.GetService<JobService>();
            Task.Run(() => ConfigureApp(jobService));
        }

        private static async Task ConfigureApp(JobService jobService)
        {
            await Cfg.LoadUserDataAsync();
            await jobService.RunAsync();
            await ElectronBootstrap.InitAsync();
        }
    }
}
