using System;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.App.Hubs;
using Bognabot.Bitmex;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http;
using Bognabot.Bitmex.Socket;
using Bognabot.Config;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Mapping;
using Bognabot.Data.Repository;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Jobs;
using Bognabot.Jobs.Init;
using Bognabot.Jobs.Sync;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace Bognabot.App
{
    public class Startup
    {
        private readonly Logger _logger; 
        private readonly IHostingEnvironment _env;
        private readonly IServiceProvider _serviceProvider;

        public Startup(IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            _env = env;
            _serviceProvider = serviceProvider;

            _logger = NLogBuilder.ConfigureNLog($"{_env.ContentRootPath}/nlog.config").GetCurrentClassLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging();

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

            Cfg.AddServices(services);
        }

        public void Configure(IApplicationBuilder app)
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
            
            Task.Run(ConfigureApp);
        }

        private async Task ConfigureApp()
        {
            await Cfg.LoadUserDataAsync(_serviceProvider, _env.ContentRootPath);

            await _serviceProvider.GetService<JobService>().RunAsync();

            await ElectronBootstrap.InitAsync();
        }
    }
}
