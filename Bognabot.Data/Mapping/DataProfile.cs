using AutoMapper;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Data.Mapping
{
    public class DataProfile : Profile
    {
        public DataProfile()
        {
            CreateMap<CandleDto, Candle>();
        }        
    }
}
