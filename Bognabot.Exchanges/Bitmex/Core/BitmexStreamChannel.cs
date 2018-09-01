using System;
using System.Linq;
using Bognabot.Config;
using Bognabot.Exchanges.Bitmex.Streams;
using Bognabot.Exchanges.Core;
using Newtonsoft.Json;

namespace Bognabot.Exchanges.Bitmex.Core
{
    public class BitmexStreamChannel<T> : IStreamChannel where T : StreamResponse
    {
        public string ChannelName { get; }

        public BitmexStreamChannel(string channelName)
        {
            ChannelName = channelName;
        }

        public string GetRequest(params string[] args)
        {
            if (!args.Any())
                throw new MissingFieldException();

            return $@"{{""op"": ""subscribe"", ""args"": [""{ChannelName}:{args[0]}""]}}";
        }

        public StreamResponse[] GetResponses(string json)
        {
            var container = JsonConvert.DeserializeObject<BitmexStreamContainer<T>>(json);
            
            return container.Data.Select(x => (StreamResponse)x).ToArray();
        }
    }
}