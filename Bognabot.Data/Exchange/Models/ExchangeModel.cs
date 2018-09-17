using System;
using Bognabot.Data.Exchange.Enums;
using Newtonsoft.Json;

namespace Bognabot.Data.Exchange.Models
{
    public abstract class ExchangeModel
    {
        public string ExchangeName { get; set; }
        public Instrument Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}