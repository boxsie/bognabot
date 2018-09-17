namespace Bognabot.Data.Exchange
{
    public interface ICollectionRequest
    {
        double Count { get; set; }
        double StartAt { get; set; }
    }
}