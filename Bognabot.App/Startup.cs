using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.App.Hubs;
using Bognabot.Config;
using Bognabot.Config.Core;
using Bognabot.Data.Models.Exchange;
using Bognabot.Exchanges.Bitmex;
using Bognabot.Exchanges.Bitmex.Core;
using Bognabot.Exchanges.Bitmex.Streams;
using Bognabot.Exchanges.Bitmex.Trade;
using Bognabot.Exchanges.Core;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var config = Config.App.BuildConfig(services);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();

            services.AddTransient<BitmexStream>();
            services.AddTransient<BitmexCommand>();
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

        private static void ConfigureApp(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            Config.App.AttachConfig(env.ContentRootPath, serviceProvider).GetAwaiter().GetResult();

            //await ElectronBootstrap.Init();

            var bds = serviceProvider.GetService<BitmexService>();

            Task.Run(bds.SubscribeToStreams);

            bds.OnTradeReceived += OnTradeReceived;
            bds.OnBookReceived += OnBookReceived;

            var candles = bds.GetCandles(TimePeriod.OneMinute, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now).GetAwaiter().GetResult();

            foreach (var candle in candles)
                Console.WriteLine($"CANDLE: {candle.Instrument.ToString().ToUpper()} - {candle.Timestamp}  - H:{candle.High} L:{candle.Low} O:{candle.Open} C:{candle.Close}");
        }

        private static Task OnBookReceived(BookModel[] arg)
        {
            foreach (var response in arg)
                Console.WriteLine($"BOOK: {response.Instrument.ToString().ToUpper()} - {response.Timestamp} - {response.Side.ToString()}:{response.Size} @ ${response.Price:N}");

            return Task.CompletedTask;
        }

        private static Task OnTradeReceived(TradeModel[] arg)
        {
            foreach (var response in arg)
                Console.WriteLine($"TRADE: {response.Instrument.ToString().ToUpper()} - {response.Timestamp} - {response.Side.ToString()}:{response.Size} @ ${response.Price:N}");

            return Task.CompletedTask;
        }
    }
}
