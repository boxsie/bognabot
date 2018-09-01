using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bognabot.Exchanges.Bitmex;
using Bognabot.Exchanges.Bitmex.Core;
using Bognabot.Net;
using Newtonsoft.Json.Linq;

namespace Bognabot.Exchanges.Core
{
    public abstract class ExchangeStream
    {
        protected abstract Uri DataUri { get; }
        protected abstract Dictionary<Type, IStreamChannel> Channels { get; }
        protected abstract string GetAuthRequest();
        protected abstract StreamResponse[] ParseResponseJson(string json);

        private readonly TextWebSocketClient _client;
        private readonly Dictionary<Type, List<Func<StreamResponse[], Task>>> _subscribers;

        protected ExchangeStream()
        {
            _client = new TextWebSocketClient(EncodingType.UTF8, OnReceived);
            _subscribers = new Dictionary<Type, List<Func<StreamResponse[], Task>>>();
        }

        public Task ConnectAsync()
        {
            return _client.ConnectAsync(DataUri);
        }

        public async Task SubscribeAsync<T>(Func<T[], Task> onReceive, params string[] args) where T : StreamResponse
        {
            var streamType = (typeof(T));
            
            if (!Channels.ContainsKey(streamType))
                return;

            var onRecFunc = new Func<StreamResponse[], Task>(x => onReceive.Invoke(x.Select(y => (T)y).ToArray()));

            if (_subscribers.ContainsKey(streamType))
            {
                var subs = _subscribers[streamType];

                subs.Add(onRecFunc);
            }
            else
            {
                _subscribers.Add(streamType, new List<Func<StreamResponse[], Task>> { onRecFunc });

                var channel = Channels[streamType];

                await _client.SendAsync(channel.GetRequest(args));
            }
        }

        private async Task OnReceived(string arg)
        {
            var baseResponse = ParseResponseJson(arg);

            if (baseResponse == null || !baseResponse.Any())
                return;

            var key = baseResponse.First().GetType();

            if (_subscribers.ContainsKey(key))
            {
                var subs = _subscribers[key];

                if (subs.Count > 0)
                    await Task.WhenAll(subs.Select(x => x.Invoke(baseResponse)));
            }
        }
    }
}
