using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Net;
using Bognabot.Net.Api;

namespace Bognabot.Data.Exchange
{
    public abstract class ExchangeSocketClient
    {
        protected abstract Uri DataUri { get; }
        protected abstract Dictionary<Type, ISocketChannel> Channels { get; }
        protected abstract string GetAuthRequest();
        protected abstract SocketResponse[] ParseResponseJson(string json);

        private readonly TextWebSocketClient _client;
        private readonly Dictionary<Type, List<Func<SocketResponse[], Task>>> _subscribers;

        protected ExchangeSocketClient()
        {
            _client = new TextWebSocketClient(EncodingType.UTF8, OnReceived);
            _subscribers = new Dictionary<Type, List<Func<SocketResponse[], Task>>>();
        }

        public Task ConnectAsync()
        {
            return _client.ConnectAsync(DataUri);
        }

        public async Task SubscribeAsync<T>(Func<T[], Task> onReceive, params string[] args) where T : SocketResponse
        {
            var streamType = (typeof(T));
            
            if (!Channels.ContainsKey(streamType))
                return;

            var onRecFunc = new Func<SocketResponse[], Task>(x => onReceive.Invoke(x.Select(y => (T)y).ToArray()));

            if (_subscribers.ContainsKey(streamType))
            {
                var subs = _subscribers[streamType];

                subs.Add(onRecFunc);
            }
            else
            {
                _subscribers.Add(streamType, new List<Func<SocketResponse[], Task>> { onRecFunc });

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
