using System;
using System.Linq;
using AutoMapper;
using Bognabot.Config;
using Bognabot.Config.Exchange;
using Bognabot.Data.Models.Exchange;
using Bognabot.Data.Repository;
using Bognabot.Domain.Entities.Instruments;
using Microsoft.Extensions.DependencyInjection;

namespace Bognabot.Data.Core
{
    public static class DataUtils
    {
        public static IMappingExpression<TSource, TDest> IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());
            
            return expression;
        }
    }
}
