using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace Bognabot.Net
{
    public class TextHttpClient : IDisposable
    {
        private readonly HttpClient _client;

        public TextHttpClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> Get(string baseUrl, IDictionary<string, string> param = null)
        {
            var queryUri = new Uri($"{_client.BaseAddress}{baseUrl}{param.BuildQueryString()}");

            var response = await _client.GetAsync(queryUri);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            return null;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    public class TextWebSocketClient : IDisposable
    {
        private readonly EncodingType _encodingType;
        private readonly Func<string, Task> _onReceived;

        private ClientWebSocket _client;
        private CancellationToken _cancellationToken;

        public TextWebSocketClient(EncodingType encodingType, Func<string, Task> onReceived)
        {
            _encodingType = encodingType;
            _onReceived = onReceived;
        }

        public async Task ConnectAsync(Uri uri)
        {
            _client = new ClientWebSocket();
            _cancellationToken = new CancellationToken();

            await _client.ConnectAsync(uri, _cancellationToken);

            Task.Run(ListenAsync, _cancellationToken).ConfigureAwait(false);
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

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
