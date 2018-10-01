using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Services.Exchange.Contracts;
using Newtonsoft.Json;
using NLog;

namespace Bognabot.Services.Exchange
{
    public interface ISocketChannel
    {
        Task<Dictionary<string, string>> GetHttpAuthHeader(HttpMethod httpMethod, string requestPath, string requestData);
        Task<string> GetSocketAuthRequest();
    }

    public abstract class ExchangeApi : IExchangeApi
    {
        public abstract DateTime Now { get; }

        protected readonly ExchangeConfig ExchangeConfig;

        private readonly ILogger _logger;
        private readonly IExchangeSocketClient _socketClient;
        private readonly Dictionary<ExchangeChannel, List<IStreamSubscription>> _subscriptions;
        private readonly Timer _authTimer;

        protected abstract Task<Dictionary<string, string>> GetHttpAuthHeader(HttpMethod httpMethod, string requestPath, string requestData);
        protected abstract Task<string> GetSocketAuthRequest();
        protected abstract Task<string> GetSocketRequest(ExchangeChannel channel, Instrument? instrument = null);
        protected abstract Task OnSocketReceive(string json);

        protected ExchangeApi(ILogger logger, ExchangeConfig config)
        {
            _logger = logger;
            ExchangeConfig = config;

            _socketClient = new ExchangeSocketClient(logger);
            _subscriptions = new Dictionary<ExchangeChannel, List<IStreamSubscription>>();

            _authTimer = new Timer((ExchangeConfig.AuthExpireSeconds * 0.99) * 1000);

            _authTimer.Elapsed += async (sender, args) => await SendWebsocketAuth(sender, null);
        }

        public async Task StartAsync()
        {
            await _socketClient.ConnectAsync(ExchangeConfig.WebSocketUrl, OnSocketReceive);

            await SendWebsocketAuth();

            _authTimer.Start();
        }

        public async Task SubscribeToSocketAsync<T>(ExchangeChannel channel, IStreamSubscription subscription, Instrument? instrument = null) where T : ExchangeDto
        {
            if (!ExchangeConfig.SupportedWebsocketChannels.ContainsKey(channel))
                return;

            if (!_subscriptions.ContainsKey(channel))
                _subscriptions.Add(channel, new List<IStreamSubscription>());

            _subscriptions[channel].Add(subscription);

            await _socketClient.SubscribeAsync(await GetSocketRequest(channel, instrument));
        }

        public async Task<T> GetAsync<T, TY>(string path, IRequest request)
            where T : ExchangeDto
            where TY : IResponse
        {
            using (var client = new ExchangeHttpClient(ExchangeConfig.RestUrl))
            {
                var query = $"?{request.AsDictionary().BuildQueryString()}";
                var authHeader = await GetHttpAuthHeader(HttpMethod.GET, path, query);

                var response = await client.GetAsync<TY>($"{path}{query}", authHeader);

                if (response == null)
                    throw new NullReferenceException();

                var model = response.Select(x => Mapper.Map<T>(x)).FirstOrDefault();

                return model;
            }
        }

        public async Task<List<T>> GetAllAsync<T, TY>(string path, ICollectionRequest request)
            where T : ExchangeDto
            where TY : IResponse
        {
            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            var returnModels = new List<T>();

            do
            {
                using (var client = new ExchangeHttpClient(ExchangeConfig.RestUrl))
                {
                    stopwatch.Restart();
                    total += count;
                    request.StartAt = total;

                    var query = $"?{request.AsDictionary().BuildQueryString()}";
                    var authHeader = await GetHttpAuthHeader(HttpMethod.GET, path, query);

                    var response = await client.GetAsync<TY>($"{path}{query}", authHeader);

                    if (response == null)
                        throw new NullReferenceException();

                    var models = response.Select(x => Mapper.Map<T>(x)).ToArray();

                    returnModels.AddRange(models);

                    count = response.Length;

                    if (count < request.Count)
                        count = 0;

                    if (stopwatch.Elapsed < TimeSpan.FromSeconds(1.01))
                        await Task.Delay(TimeSpan.FromSeconds(1.5).Subtract(stopwatch.Elapsed));
                }

            } while (count > 0);

            return returnModels;
        }

        public async Task<T> PostAsync<T, TY>(string path, IRequest request)
            where T : ExchangeDto
            where TY : IResponse
        {
            using (var client = new ExchangeHttpClient(ExchangeConfig.RestUrl))
            {
                var data = request.AsDictionary().BuildQueryString();
                var authHeader = await GetHttpAuthHeader(HttpMethod.POST, path, data);

                var response = await client.PostAsync<TY>(path, data, authHeader);

                if (response == null)
                    throw new NullReferenceException();

                var model = response.Select(x => Mapper.Map<T>(x)).FirstOrDefault();

                return model;
            }
        }

        public Instrument? ToInstrumentType(string symbol)
        {
            var instrumentKvp = ExchangeConfig.SupportedInstruments.FirstOrDefault(x => x.Value == symbol);

            if (instrumentKvp.Value == null)
                throw new ArgumentOutOfRangeException();

            return instrumentKvp.Key;
        }

        public string ToSymbol(Instrument instrument)
        {
            var supportedInstruments = ExchangeConfig.SupportedInstruments;

            return supportedInstruments.ContainsKey(instrument)
                ? supportedInstruments[instrument]
                : throw new ArgumentOutOfRangeException();
        }

        public string ToTimePeriod(TimePeriod period)
        {
            var supportedPeriods = ExchangeConfig.SupportedTimePeriods;

            return supportedPeriods.ContainsKey(period)
                ? supportedPeriods[period]
                : throw new ArgumentOutOfRangeException();
        }

        protected async Task UpdateSubscriptions(ExchangeChannel channel, IEnumerable models)
        {
            if (_subscriptions.ContainsKey(channel))
            {
                var subs = _subscriptions[channel];

                foreach (var subscription in subs)
                    await subscription.TriggerUpdate(models);
            }
        }

        private async Task SendWebsocketAuth(object sender = null, ElapsedEventArgs e = null)
        {
            var authRequest = await GetSocketAuthRequest();

            await _socketClient.SendAsync(authRequest);
        }
    }
}