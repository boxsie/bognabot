using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Websockets;
using Newtonsoft.Json;

namespace Bognabot.Bitmex
{
    public class BitmexService
    {
        private readonly BitmexWebsocketClient _client;

        private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);

        public async Task StartAsync()
        {
            var url = Settings.BitmexApiUri;

            using (var communicator = new BitmexWebsocketCommunicator(url))
            {
                communicator.ReconnectTimeoutMs = (int) TimeSpan.FromSeconds(30).TotalMilliseconds;
                communicator.ReconnectionHappened.Subscribe(type =>
                    Console.WriteLine($"Reconnection happened, type: {type}"));

                using (var client = new BitmexWebsocketClient(communicator))
                {
                    client.Streams.InfoStream.Subscribe(async info =>
                    {
                        Console.WriteLine($"Reconnection happened, Message: {info.Info}, Version: {info.Version:D}");
                        await SendSubscriptionRequests(client);
                    });
                    
                    SubscribeToStreams(client);

                    await communicator.Start();

                    ExitEvent.WaitOne();
                }
            }
        }

        public async Task SubscribeAsync()
        {

        }

        private static async Task SendSubscriptionRequests(BitmexWebsocketClient client)
        {
            //await client.Send(new BookSubscribeRequest());
            await client.Send(new TradesSubscribeRequest("XBTUSD"));
            await client.Send(new TradeBinSubscribeRequest("1m", "XBTUSD"));
            await client.Send(new TradeBinSubscribeRequest("5m", "XBTUSD"));
            await client.Send(new QuoteSubscribeRequest("XBTUSD"));
            await client.Send(new LiquidationSubscribeRequest());

            if (!string.IsNullOrWhiteSpace(Settings.BitmexApiSecret))
                await client.Send(new AuthenticationRequest(Settings.BitmexApiKey, Settings.BitmexApiSecret));
        }

        private static void SubscribeToStreams(BitmexWebsocketClient client)
        {
            client.Streams.ErrorStream.Subscribe(x =>
                Console.WriteLine($"Error received, message: {x.Error}, status: {x.Status}"));

            client.Streams.AuthenticationStream.Subscribe(x =>
            {
                Console.WriteLine($"Authentication happened, success: {x.Success}");
                client.Send(new WalletSubscribeRequest()).Wait();
                client.Send(new OrderSubscribeRequest()).Wait();
                client.Send(new PositionSubscribeRequest()).Wait();
            });


            client.Streams.SubscribeStream.Subscribe(x =>
                Console.WriteLine($"Subscribed ({x.Success}) to {x.Subscribe}"));
            
            client.Streams.WalletStream.Subscribe(y =>
                y.Data.ToList().ForEach(x =>
                    Console.WriteLine($"Wallet {x.Account}, {x.Currency} amount: {x.BalanceBtc}"))
            );

            client.Streams.OrderStream.Subscribe(y =>
                y.Data.ToList().ForEach(x =>
                    Console.WriteLine(
                        $"Order {x.Symbol} updated. Time: {x.Timestamp:HH:mm:ss.fff}, Amount: {x.OrderQty}, " +
                        $"Price: {x.Price}, Direction: {x.Side}, Working: {x.WorkingIndicator}, Status: {x.OrdStatus}"))
            );

            client.Streams.PositionStream.Subscribe(y =>
                y.Data.ToList().ForEach(x =>
                    Console.WriteLine(
                        $"Position {x.Symbol}, {x.Currency} updated. Time: {x.Timestamp:HH:mm:ss.fff}, Amount: {x.CurrentQty}, " +
                        $"Price: {x.LastPrice}, PNL: {x.UnrealisedPnl}"))
            );

            client.Streams.TradesStream.Subscribe(y =>
                y.Data.ToList().ForEach(x =>
                    Console.WriteLine($"Trade {x.Symbol} executed. Time: {x.Timestamp:mm:ss.fff}, Amount: {x.Size}, " +
                                    $"Price: {x.Price}, Direction: {x.TickDirection}"))
            );

            client.Streams.BookStream.Subscribe(book =>
                book.Data.Take(100).ToList().ForEach(x => Console.WriteLine(
                    $"Book | {book.Action} pair: {x.Symbol}, price: {x.Price}, amount {x.Size}, side: {x.Side}"))
            );

            client.Streams.QuoteStream.Subscribe(y =>
                y.Data.ToList().ForEach(x =>
                    Console.WriteLine(
                        $"Quote {x.Symbol}. Bid: {x.BidPrice} - {x.BidSize} Ask: {x.AskPrice} - {x.AskSize}"))
            );

            client.Streams.LiquidationStream.Subscribe(y =>
                y.Data.ToList().ForEach(x =>
                    Console.WriteLine(
                        $"Liquadation Action:{y.Action} OrderID:{x.OrderID} Symbol:{x.Symbol} Side:{x.Side} Price:{x.Price} leavesQty:{x.leavesQty}"))
            );

            client.Streams.TradeBinStream.Subscribe(y =>
                y.Data.ToList().ForEach(x =>
                    Console.WriteLine(
                        $"TradeBin Table:{y.Table} {x.Symbol} executed. Time: {x.Timestamp:mm:ss.fff}, Open: {x.Open}, " +
                        $"Close: {x.Close}, Volume: {x.Volume}"))
            );

        }
    }
    
    public static class TypeExtensions
    {
        public static List<Type> GetAllDerivedTypes(this Type type)
        {
            return Assembly.GetAssembly(type).GetAllDerivedTypes(type);
        }

        public static List<Type> GetAllDerivedTypes(this Assembly assembly, Type type)
        {
            return assembly
                .GetTypes()
                .Where(t => t != type && type.IsAssignableFrom(t))
                .ToList();
        }
    }
}


