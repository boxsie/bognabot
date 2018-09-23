using System;
using Bognabot.Data.Exchange.Enums;
using Newtonsoft.Json;

namespace Bognabot.Data.Exchange.Dtos
{
    public abstract class ExchangeDto
    {
        public string ExchangeName { get; set; }
        public Instrument Instrument { get; set; }
        public DateTime Timestamp { get; set; }
    }
}