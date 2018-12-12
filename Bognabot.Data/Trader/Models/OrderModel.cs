using System;
using System.Collections.Generic;
using System.Text;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Trader.Models
{
    public class OrderModel
    {
        public string Exchange { get; set; }
        public double Amount { get; set; }
        public Instrument Instrument { get; set; }
        public double Price { get; set; }
        public TradeSide Side { get; set; }
        public OrderType OrderType { get; set; }
        public double OrderStopAmount { get; set; }
        public double OrderProfitAmount { get; set; }
        public double OrderLimitSlipAmount { get; set; }
    }
}
