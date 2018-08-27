using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bognabot.Bitmex
{
    public class BitmexWebsocketClient : IDisposable
    {
        private ClientWebSocket _client;
        private CancellationTokenSource _cancellation;

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task StartAsync(CancellationToken token)
        {
            _cancellation = new CancellationTokenSource();

            await GetClient().ConnectAsync(Settings.BitmexApiUri, token);
        }

        public async Task Subscribe(BitmexSubject subject)
        {
            var ticker = ":XBTUSD";

            switch (subject)
            {
                case BitmexSubject.Announcement:
                case BitmexSubject.Chat:
                case BitmexSubject.Connected:
                case BitmexSubject.PublicNotifications:
                case BitmexSubject.PrivateNotifications:
                    ticker = "";
                    break;
            }

            await SendAsync($@"{{""op"": ""subscribe"", ""args"": [""{subject.ToString().ToLower()}{ticker}""]}}");
        }

        public async Task AuthenticateAsync(string apiKey, string signature)
        {
            await SendAsync($@"{{""op"": ""authKeyExpires"", ""args"": [""{apiKey}"", {BitmexTime.Expires()}, ""{signature}""]}}");
        }

        public async Task ListenAsync(CancellationToken token)
        {
            do
            {
                WebSocketReceiveResult result = null;

                var buffer = new byte[1000];
                var message = new ArraySegment<byte>(buffer);
                var resultMessage = new StringBuilder();

                do
                {
                    result = await GetClient().ReceiveAsync(message, token);

                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    resultMessage.Append(receivedMessage);

                    if (result.MessageType != WebSocketMessageType.Text)
                        break;

                } while (!result.EndOfMessage);

                var received = resultMessage.ToString();

                Console.WriteLine(received);
            } while (GetClient().State == WebSocketState.Open && !token.IsCancellationRequested);
        }
        
        private async Task SendAsync(string message)
        {
            try
            {
                var msgBytes = Encoding.UTF8.GetBytes(message);
                var bytes = new ArraySegment<byte>(msgBytes);

                await GetClient().SendAsync(bytes, WebSocketMessageType.Text, true, _cancellation.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private ClientWebSocket GetClient()
        {
            return _client ?? (_client = new ClientWebSocket
            {
                Options = {KeepAliveInterval = new TimeSpan(0, 0, 5, 0)}
            });
        }
    }
}