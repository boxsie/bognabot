using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using HttpMethod = Bognabot.Data.Exchange.Enums.HttpMethod;

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

        public async Task<T[]> GetAsync<T>(string path, Dictionary<string, string> authHeaders = null)
        {
            _logger.Log(LogLevel.Debug, $"{HttpMethod.GET} {path}");

            if (authHeaders != null)
            {
                foreach (var authHeader in authHeaders)
                    DefaultRequestHeaders.Add(authHeader.Key, authHeader.Value);
            }

            var url = $"{BaseAddress}{path}";

            var response = await GetAsync(url);

            _logger.Log(LogLevel.Debug, $"{HttpMethod.GET} {url}");
            _logger.Log(response.IsSuccessStatusCode ? LogLevel.Debug : LogLevel.Error, $"{HttpMethod.GET} {path} {response.ReasonPhrase}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var isArray = JToken.Parse(json) is JArray;

            return isArray ? JsonConvert.DeserializeObject<T[]>(json) : new[] { JsonConvert.DeserializeObject<T>(json) };
        }

        public async Task<T[]> PostAsync<T>(string path, string request, Dictionary<string, string> authHeaders = null)
        {
            try
            {
                _logger.Log(LogLevel.Debug, $"{HttpMethod.POST} {path}");

                if (authHeaders != null)
                {
                    foreach (var authHeader in authHeaders)
                        DefaultRequestHeaders.Add(authHeader.Key, authHeader.Value);
                }

                var content = new StringContent(request, Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await PostAsync($"{BaseAddress}{path}", content);

                _logger.Log(LogLevel.Debug, $"{HttpMethod.POST} {path}");
                _logger.Log(response.IsSuccessStatusCode ? LogLevel.Debug : LogLevel.Error, $"{HttpMethod.POST} {path} {response.ReasonPhrase}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var isArray = JToken.Parse(json) is JArray;

                return isArray ? JsonConvert.DeserializeObject<T[]>(json) : new []{ JsonConvert.DeserializeObject<T>(json) };
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e);
                throw;
            }
            
        }
    }
}