using System;
using System.Threading.Tasks;
using Bognabot.Bitmex;

namespace Bognabot.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var bs = new BitmexService();

            Task.Run(async () => await bs.StartAsync());

            bs.OnPriceUpdate += BsOnOnPriceUpdate;

            System.Console.Read();
        }

        private static void BsOnOnPriceUpdate(double obj)
        {
            

        }
    }
}
