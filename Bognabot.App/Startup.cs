using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.App.Hubs;
using Bognabot.Bitmex;
using Bognabot.Config;
using Bognabot.Config.Core;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bognabot.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var config = AppConfig.BuildConfig(services);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();

            services.AddSingleton<BitmexService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
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

            ConfigureApp(app, env, serviceProvider);
        }

        private static async void ConfigureApp(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            await AppConfig.AttachConfig(env, serviceProvider);

#if !DEBUG
            await ElectronBootstrap.Init;
#endif
            await app.ApplicationServices.GetService<BitmexService>().StartAsync();
        }
    }
}
