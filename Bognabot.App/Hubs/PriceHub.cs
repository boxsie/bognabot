using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using System.Threading.Tasks;
using Bognabot.Bitmex;
using Microsoft.AspNetCore.SignalR;

namespace Bognabot.App.Hubs
{
    public class PriceData
    {
        public double Last { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
    }

    public class PriceControl
    {
        private readonly IHubContext<StreamHub> _hub;
        private readonly BitmexService _bitmexService;
        private readonly Subject<PriceData> _latestPrice;

        public PriceControl(IHubContext<StreamHub> hub, BitmexService bitmexService)
        {
            _hub = hub;
            _bitmexService = bitmexService;
            _latestPrice = new Subject<PriceData>();
        }

        public IObservable<PriceData> StreamLatestPrice()
        {
            return _latestPrice;
        }
    }

    public class StreamHub : Hub
    {
        private readonly PriceControl _priceControl;

        public StreamHub(PriceControl priceControl)
        {
            _priceControl = priceControl;
        }

        public ChannelReader<PriceData> StreamStocks()
        {
            return _priceControl.StreamLatestPrice().AsChannelReader(10);
        }
    }

    public static class ObservableExtensions
    {
        public static ChannelReader<T> AsChannelReader<T>(this IObservable<T> observable, int? maxBufferSize = null)
        {
            // This sample shows adapting an observable to a ChannelReader without 
            // back pressure, if the connection is slower than the producer, memory will
            // start to increase.

            // If the channel is bounded, TryWrite will return false and effectively
            // drop items.

            // The other alternative is to use a bounded channel, and when the limit is reached
            // block on WaitToWriteAsync. This will block a thread pool thread and isn't recommended and isn't shown here.
            var channel = maxBufferSize != null ? Channel.CreateBounded<T>(maxBufferSize.Value) : Channel.CreateUnbounded<T>();

            var disposable = observable.Subscribe(
                value => channel.Writer.TryWrite(value),
                error => channel.Writer.TryComplete(error),
                () => channel.Writer.TryComplete());

            // Complete the subscription on the reader completing
            channel.Reader.Completion.ContinueWith(task => disposable.Dispose());

            return channel.Reader;
        }
    }
}
