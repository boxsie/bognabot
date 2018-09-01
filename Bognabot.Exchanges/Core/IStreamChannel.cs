using Bognabot.Exchanges.Bitmex.Core;

namespace Bognabot.Exchanges.Core
{
    public interface IStreamChannel
    {
        string ChannelName { get; }

        string GetRequest(params string[] args);
        StreamResponse[] GetResponses(string json);
    }
}