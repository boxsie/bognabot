using System;

namespace Bognabot.Data.Exchange
{
    public interface IResponse
    {
        DateTime Timestamp { get; set; }
    }

    public interface ICollectionRequest
    {
        double Count { get; set; }
        double StartAt { get; set; }
    }

    public interface IRequest
    {

    }
}