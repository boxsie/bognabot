using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Bognabot.Bitmex;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Trader.Enums;
using Bognabot.Trader;
using Microsoft.AspNetCore.SignalR;

namespace Bognabot.App.Hubs
{
    public class SignalMessage
    {
        public string SignalId { get; set; }
        public TimePeriod TimePeriod { get; set; }
        public SignalStrength SignalStrength { get; set; }
    }

    public class IndicatorMessage
    {
        public string IndicatorName { get; set; }
        public string ExchangeName { get; set; }
        public Instrument Instrument { get; set; }
        public TimePeriod Period { get; set; }
        public double Current { get; set; }
    }

    public class TradeHubControl
    {
        private readonly IHubContext<ExchangeInstrumentHub> _hub;
        private Dictionary<string, Channel<IndicatorMessage>> _indicatorChannels;

        public TradeHubControl(IHubContext<ExchangeInstrumentHub> hub)
        {
            _hub = hub;
            _indicatorChannels = new Dictionary<string, Channel<IndicatorMessage>>();
        }

        public ChannelReader<IndicatorMessage> StreamIndicator()
        {
            var channel = Channel.CreateUnbounded<IndicatorMessage>();


            return channel.Reader;
        }
    }

    public class ExchangeInstrumentHub : Hub
    {
        private readonly TradeHubControl _tradeHubControl;

        public ExchangeInstrumentHub(TradeHubControl tradeHubControl)
        {
            _tradeHubControl = tradeHubControl;
        }

        public ChannelReader<IndicatorMessage> StreamIndicator(string indicatorName)
        {
            return _tradeHubControl.StreamIndicator();
        }
    }

    public class OrdersHubControl
    {
        //private readonly IHubContext<OrdersHub> _hub;
        //private Dictionary<string, Channel<PositionMessage>> _positionChannels;

        //public ChannelReader<List<PositionMessage>> StreamPositions(string positionId)
        //{
        //    var channel = Channel.CreateUnbounded<IndicatorMessage>();
            
        //    return channel.Reader;
        //}

        //public Task<List<PositionMessage>> GetPositions()
        //{

        //}
    }

    //public class PositionMessage
    //{
    //    public string PositionId => HubUtils.GetInstrumentId(ExchangeName, Instrument);

    //    public string ExchangeName { get; set; }
    //    public Instrument Instrument { get; set; }

    //}
    
    //public class OrdersHub : Hub
    //{
    //    public ChannelReader<IndicatorMessage> StreamPosition(string exchange, Instrument instrument)
    //    {
    //        return _tradeHubControl.StreamIndicator();
    //    }
    //}
}
