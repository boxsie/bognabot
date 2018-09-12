using System;
using System.Linq;
using Bognabot.Bitmex.Socket.Responses;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Newtonsoft.Json;

namespace Bognabot.Bitmex.Socket
{
    public class SocketChannel<T> : ISocketChannel where T : SocketResponse
    {
        public string ChannelName { get; }

        public SocketChannel(string channelName)
        {
            ChannelName = channelName;
        }

        public string GetRequest(params string[] args)
        {
            if (!args.Any())
                throw new MissingFieldException();

            return $@"{{""op"": ""subscribe"", ""args"": [""{ChannelName}:{args[0]}""]}}";
        }

        public SocketResponse[] GetResponses(string json)
        {
            var container = JsonConvert.DeserializeObject<SocketResponse<T>>(json);
            
            return container.Data.Select(x => (SocketResponse)x).ToArray();
        }
    }
}