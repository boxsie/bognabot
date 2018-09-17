using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Enums;
using NLog;

namespace Bognabot.Services.Exchange
{
    public class ExchangeSocketClient : IExchangeSocketClient
    {
        private readonly ILogger _logger;
        private readonly EncodingType _encodingType;
        private readonly List<string> _subscribedRequests;

        private ClientWebSocket _client;
        private CancellationToken _cancellationToken;
        private Func<string, Task> _onReceive;

        public ExchangeSocketClient(ILogger logger)
        {
            _logger = logger;
            _encodingType = EncodingType.UTF8;
            _subscribedRequests = new List<string>();
        }

        public async Task ConnectAsync(string url, Func<string, Task> onReceive)
        {
            _onReceive = onReceive;

            _client?.Dispose();
            _client = new ClientWebSocket();
            _cancellationToken = new CancellationToken();

            await _client.ConnectAsync(new Uri(url), _cancellationToken);

#pragma warning disable CS4014
            Task.Run(ListenAsync, _cancellationToken).ConfigureAwait(false);
#pragma warning restore CS4014
        }

        public async Task SubscribeAsync(string request)
        {
            if (_subscribedRequests.Any(x => x == request))
                return;
            
            _subscribedRequests.Add(request);

            await SendAsync(request);
        }

        public async Task SendAsync(string message)
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

                    var responseText = response.ToString();

                    await _onReceive.Invoke(responseText);
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
