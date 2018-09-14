using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bognabot.Data.Exchange;
using Newtonsoft.Json;
using NLog;

namespace Bognabot.Services.Exchange
{
    public class ExchangeHttpClient : HttpClient
    {
        private readonly ILogger _logger;

        public ExchangeHttpClient(string baseUrl)
        {
            _logger = LogManager.GetCurrentClassLogger();
            
            BaseAddress = new Uri(baseUrl);
        }

        public async Task<T[]> GetAsync<T>(string path, string urlQuery, Dictionary<string, string> authHeaders = null)
        {
            _logger.Log(LogLevel.Debug, $"{HttpMethod.Get} {path}");

            if (authHeaders != null)
            {
                foreach (var authHeader in authHeaders)
                    DefaultRequestHeaders.Add(authHeader.Key, authHeader.Value);
            }

            var queryUri = new Uri($"{BaseAddress}{path}{urlQuery}");

            var response = await GetAsync(queryUri);

            _logger.Log(LogLevel.Debug, $"{HttpMethod.Get} {path} {queryUri}");
            _logger.Log(response.IsSuccessStatusCode ? LogLevel.Debug : LogLevel.Error, $"{HttpMethod.Get} {path} {response.ReasonPhrase}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T[]>(json);
        }
    }
}