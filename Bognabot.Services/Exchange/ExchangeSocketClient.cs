using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Services.Exchange
{
    public interface IExchangeSocketClient
    {
        Task ConnectAsync();
        Task SubscribeAsync<T>(Func<T[], Task> onReceive, params string[] args) where T : SocketResponse;
        Task ListenAsync();
        Task SendAsync(string message);
    }

    public abstract class ExchangeSocketClient
    {
        protected abstract Uri DataUri { get; }
        protected abstract Dictionary<string, ISocketChannel> Channels { get; }
        protected abstract string GetAuthRequest();
        protected abstract SocketResponse[] ParseResponseJson(string json);

        private readonly EncodingType _encodingType;
        private readonly Func<string, Task> _onReceived;
        private readonly Dictionary<Type, List<Func<SocketResponse[], Task>>> _subscribers;

        private ClientWebSocket _client;
        private CancellationToken _cancellationToken;

        protected ExchangeSocketClient()
        {
            _subscribers = new Dictionary<Type, List<Func<SocketResponse[], Task>>>();

            _encodingType = EncodingType.UTF8;
            _onReceived = OnReceived;
        }

        public async Task ConnectAsync()
        {
            _client?.Dispose();
            _client = new ClientWebSocket();
            _cancellationToken = new CancellationToken();

            await _client.ConnectAsync(DataUri, _cancellationToken);

            Task.Run(ListenAsync, _cancellationToken).ConfigureAwait(false);
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

                await SendAsync(channel.GetRequest(args));
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

        private async Task SendAsync(string message)
        {
            var buffer = NetUtils.EncodeText(message, _encodingType);
            var messageSegment = new ArraySegment<byte>(buffer);

            await _client.SendAsync(messageSegment, WebSocketMessageType.Text, true, _cancellationToken);
        }

        private async Task ListenAsync()
        {
            try
            {
                while (_client.State == WebSocketState.Open && !_cancellationToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = null;

                    var buffer = new byte[1000];
                    var message = new ArraySegment<byte>(buffer);
                    var response = new StringBuilder();

                    while (result == null || !result.EndOfMessage)
                    {
                        result = await _client.ReceiveAsync(message, _cancellationToken);

                        response.Append(NetUtils.DecodeText(buffer, result.Count, _encodingType));

                        if (result.MessageType != WebSocketMessageType.Text)
                            break;
                    }

                    if (_onReceived != null)
                        await _onReceived.Invoke(response.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
