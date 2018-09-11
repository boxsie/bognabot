using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http.Commands;
using Bognabot.Bitmex.Http.Requests;
using Bognabot.Bitmex.Socket;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Net;
using Bognabot.Services.Exchange;
using Bognabot.Storage.Core;
using NLog;

namespace Bognabot.Bitmex.Http
{
    public class BitmexHttpClient : ExchangeHttpClient
    {
        protected override Dictionary<Type, IHttpCommand> Commands { get; }

        private readonly ExchangeConfig _config;
        
        public BitmexHttpClient(ILogger logger, ExchangeConfig config) : base(logger)
        {
            _config = config;

            Commands = new Dictionary<Type, IHttpCommand>
            {
                { typeof(TradeCommandRequest), new TradeCommand() }
            };
        }

        protected override void AddAuthHeaders<T>(T request, HttpClient client, string urlQuery)
        {
            client.BaseAddress = new Uri(_config.RestUrl);

            if (!request.IsAuth)
                return;

            var signatureMessage = $"{request.HttpMethod.ToString()}/api/v1{request.Path}{urlQuery}{BitmexUtils.Expires()}";
            var signatureBytes = StorageUtils.EncryptHMACSHA256(Encoding.UTF8.GetBytes(_config.UserConfig.Secret), Encoding.UTF8.GetBytes(signatureMessage));

            client.DefaultRequestHeaders.Add("api-expires", BitmexUtils.Expires().ToString());
            client.DefaultRequestHeaders.Add("api-key", _config.UserConfig.Key);
            client.DefaultRequestHeaders.Add("api-signature", StorageUtils.ByteArrayToHexString(signatureBytes));
        }
    }
}