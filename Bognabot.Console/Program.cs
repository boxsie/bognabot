using System;
using System.Threading.Tasks;
using Bognabot.Bitmex;

namespace Bognabot.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            Task.Run(async () => await new App().Run());

            System.Console.Read();
        }
    }

    public class App
    {
        public async Task Run()
        {
            var bs = new BitmexService();

            await bs.StartAsync();

            System.Console.WriteLine("MOOOP");
        }
    }
}
