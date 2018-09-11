using AutoMapper;
using Newtonsoft.Json;

namespace Bognabot.Data
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
