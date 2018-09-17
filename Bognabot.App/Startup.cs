using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.App.Hubs;
using Bognabot.Bitmex;
using Bognabot.Bitmex.Socket;
using Bognabot.Core;
using Bognabot.Data;
using Bognabot.Data.Config;
using Bognabot.Data.Config.Contracts;
using Bognabot.Data.Mapping;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services;
using Bognabot.Services.Exchange;
using Bognabot.Services.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Web;

namespace Bognabot.App
{
    public class Startup
    {
        private readonly Logger _logger; 
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            _logger = NLogBuilder.ConfigureNLog($"{_env.ContentRootPath}/nlog.config").GetCurrentClassLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger>((x) => _logger);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            services.AddSignalR();

            AppInitialise.AddServices(services, new [] { typeof(BitmexService) });
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

            ConfigureApp(serviceProvider);
        }

        private void ConfigureApp(IServiceProvider serviceProvider)
        {
            AppInitialise.LoadUserData(serviceProvider, _env.ContentRootPath);

            var sm = serviceProvider.GetService<ServiceManager>();

            Task.Run(() => AppInitialise.Start(sm));
            Task.Run(ElectronBootstrap.InitAsync);
        }
    }
}
