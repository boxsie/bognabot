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

            AddHeaders(authHeaders);

            var url = $"{BaseAddress}{path}";

            var response = await GetAsync(url);

            return await DeserialiseResponse<T>(response, HttpMethod.GET);
        }

        public async Task<T[]> PostAsync<T>(string path, string request, Dictionary<string, string> authHeaders = null)
        {
            _logger.Log(LogLevel.Debug, $"{HttpMethod.POST} {path}");

            AddHeaders(authHeaders);

            var content = new StringContent(request, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await PostAsync($"{BaseAddress}{path}", content);

            return await DeserialiseResponse<T>(response, HttpMethod.POST);
        }

        private void AddHeaders(Dictionary<string, string> authHeaders = null)
        {
            if (authHeaders == null)
                return;

            foreach (var authHeader in authHeaders)
                DefaultRequestHeaders.Add(authHeader.Key, authHeader.Value);
        }

        private async Task<T[]> DeserialiseResponse<T>(HttpResponseMessage response, HttpMethod method)
        {
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Log(LogLevel.Error, $"{method} {response.ReasonPhrase}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var isArray = JToken.Parse(json) is JArray;

                _logger.Log(LogLevel.Debug, $"{method} {response.ReasonPhrase}");

                return isArray
                    ? JsonConvert.DeserializeObject<T[]>(json)
                    : new[] {JsonConvert.DeserializeObject<T>(json)};
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e);
                return null;
            }
        }
    }
}


        