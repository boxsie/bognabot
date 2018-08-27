using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bognabot.Bitmex
{
    public class BitmexService
    {
        public event Action<double> OnPriceUpdate;

        private readonly BitmexWebsocketClient _client;
        private CancellationToken _token;

        public BitmexService()
        {
            _client = new BitmexWebsocketClient();
        }

        public async Task StartAsync()
        {
            _token = new CancellationToken(false);
            
            await _client.StartAsync(_token);

            await AuthenticateAsync("iiMc1cci1lNhyVpmrILHqCRY", "nkj7IJuPVugn8kYmIqXQ093ho7Z8ccipeNbn6RTZrosnZCUh");

            await _client.Subscribe(BitmexSubject.Instrument);

            await _client.ListenAsync(_token);
        }

        public async Task AuthenticateAsync(string apiKey, string apiSecret)
        {
            await _client.AuthenticateAsync(apiKey, BitmexAuthentication.CreateSignature(apiSecret));
        }
    }
}
