using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bognabot.Net;
using Newtonsoft.Json;

namespace Bognabot.Exchanges.Core
{
    public abstract class ExchangeCommand
    {
        protected abstract Dictionary<Type, ICommand> Commands { get; }

        protected abstract TextHttpClient GetClient(bool isAuth);

        public async Task<TY[]> GetAsync<T, TY>(T request) where T : CommandRequest where TY : CommandResponse
        {
            using (var client = GetClient(request.IsAuth))
            {
                var cmdType = typeof(T);

                if (!Commands.ContainsKey(cmdType))
                    return null;

                var cmd = Commands[cmdType];

                var response = await client.Get(request.Path, cmd.GetRequestParams(request));

                return JsonConvert.DeserializeObject<TY[]>(response);
            }
        }
    }
}