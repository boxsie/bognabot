using AutoMapper;
using Bognabot.Data.Exchange.Models;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Data.Mapping
{
    public class DataProfile : Profile
    {
        public DataProfile()
        {
            CreateMap<CandleModel, Candle>();
        }        
    }
}
