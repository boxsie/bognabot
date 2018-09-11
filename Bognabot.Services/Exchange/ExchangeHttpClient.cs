using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bognabot.Data.Exchange;
using Bognabot.Net;
using Newtonsoft.Json;
using NLog;

namespace Bognabot.Services.Exchange
{
    public abstract class ExchangeHttpClient
    {
        private readonly ILogger _logger;

        protected ExchangeHttpClient(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract Dictionary<Type, IHttpCommand> Commands { get; }
        protected abstract void AddAuthHeaders<T>(T request, HttpClient client, string urlQuery) where T : CommandRequest;

        public async Task<TY[]> GetAsync<T, TY>(T request) where T : CommandRequest where TY : CommandResponse
        {
            _logger.Log(LogLevel.Debug, $"{request.HttpMethod} {request.GetType().Name}");

            using (var client = new HttpClient())
            {
                var cmdType = typeof(T);

                if (!Commands.ContainsKey(cmdType))
                    return null;

                var cmd = Commands[cmdType];

                var urlQuery = cmd.GetRequestParams(request).BuildQueryString();

                AddAuthHeaders(request, client, urlQuery);

                var queryUri = new Uri($"{client.BaseAddress}{request.Path}{urlQuery}");

                var response = await client.GetAsync(queryUri);

                _logger.Log(LogLevel.Debug, $"{request.HttpMethod} {request.GetType().Name} {queryUri}");
                _logger.Log(response.IsSuccessStatusCode ? LogLevel.Info : LogLevel.Error, $"{request.HttpMethod} {request.GetType().Name} {response.ReasonPhrase}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TY[]>(json);
            }
        }
    }
}