namespace Bognabot.Data.Exchange
{
    public interface ISocketChannel
    {
        string ChannelName { get; }

        string GetRequest(params string[] args);
        SocketResponse[] GetResponses(string json);
    }
}