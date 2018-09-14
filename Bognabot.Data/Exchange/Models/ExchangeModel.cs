using System;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Data.Exchange.Models
{
    public abstract class ExchangeModel
    {
        public Instrument Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}