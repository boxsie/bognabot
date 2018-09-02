using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http.Commands;
using Bognabot.Bitmex.Http.Requests;
using Bognabot.Bitmex.Socket;
using Bognabot.Config;
using Bognabot.Data.Exchange;
using Bognabot.Net;
using Bognabot.Net.Api;
using Bognabot.Storage.Core;
using Microsoft.Extensions.Logging;
using HttpMethod = Bognabot.Net.HttpMethod;

namespace Bognabot.Bitmex.Http
{
    public class BitmexHttpClient : ExchangeHttpClient
    {
        protected override Dictionary<Type, IHttpCommand> Commands { get; }

        public BitmexHttpClient(ILogger<BitmexHttpClient> logger) : base(logger)
        {
            Commands = new Dictionary<Type, IHttpCommand>
            {
                { typeof(TradeCommandRequest), new TradeCommand() }
            };
        }

        protected override void AddAuthHeaders<T>(T request, HttpClient client, string urlQuery)
        {
            client.BaseAddress = new Uri(Cfg.Exchange.App.Bitmex.RestUrl);

            if (!request.IsAuth)
                return;

            var signatureMessage = $"{request.HttpMethod.ToString()}/api/v1{request.Path}{urlQuery}{BitmexUtils.Expires()}";
            var signatureBytes = StorageUtils.EncryptHMACSHA256(Encoding.UTF8.GetBytes(Cfg.Exchange.User.Bitmex.Secret), Encoding.UTF8.GetBytes(signatureMessage));

            client.DefaultRequestHeaders.Add("api-expires", BitmexUtils.Expires().ToString());
            client.DefaultRequestHeaders.Add("api-key", Cfg.Exchange.User.Bitmex.Key);
            client.DefaultRequestHeaders.Add("api-signature", StorageUtils.ByteArrayToHexString(signatureBytes));
        }
    }
}