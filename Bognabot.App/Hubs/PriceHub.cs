using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Bognabot.Bitmex;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Trader;
using Microsoft.AspNetCore.SignalR;

namespace Bognabot.App.Hubs
{
    public class SignalData
    {
        public string SignalId { get; set; }
        public TimePeriod TimePeriod { get; set; }
        public SignalStrength SignalStrength { get; set; }
    }

    public class SignalsControl
    {
        private readonly IHubContext<StreamHub> _hub;

        public SignalsControl(IHubContext<StreamHub> hub)
        {
            _hub = hub;
        }

        public IObservable<SignalData> StreamLatestPrice()
        {
            return null;
        }
    }

    public class StreamHub : Hub
    {
        private readonly SignalsControl _signalControl;

        public StreamHub(SignalsControl signalControl)
        {
            _signalControl = signalControl;
        }

        public ChannelReader<SignalData> StreamStocks()
        {
            return null;// _priceControl.StreamLatestPrice().AsChannelReader(10);
        }
    }

    public static class ObservableExtensions
    {
        public static ChannelReader<T> AsChannelReader<T>(this IObservable<T> observable, int? maxBufferSize = null)
        {
            // This sample shows adapting an observable to a ChannelReader without 
            // back pressure, if the connection is slower than the producer, memory will
            // start to increase.

            //// If the channel is bounded, TryWrite will return false and effectively
            //// drop items.

            //// The other alternative is to use a bounded channel, and when the limit is reached
            //// block on WaitToWriteAsync. This will block a thread pool thread and isn't recommended and isn't shown here.
            //var channel = maxBufferSize != null ? Channel.CreateBounded<T>(maxBufferSize.Value) : Channel.CreateUnbounded<T>();

            //var disposable = observable.Subscribe(
            //    value => channel.Writer.TryWrite(value),
            //    error => channel.Writer.TryComplete(error),
            //    () => channel.Writer.TryComplete());

            //// Complete the subscription on the reader completing
            //channel.Reader.Completion.ContinueWith(task => disposable.Dispose());

            return null; //channel.Reader;
        }
    }
}
