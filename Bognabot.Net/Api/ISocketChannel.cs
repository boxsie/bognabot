namespace Bognabot.Net.Api
{
    public interface ISocketChannel
    {
        string ChannelName { get; }

        string GetRequest(params string[] args);
        SocketResponse[] GetResponses(string json);
    }
}