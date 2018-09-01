using Newtonsoft.Json;

namespace Bognabot.Exchanges.Bitmex.Streams
{
    public class BitmexStreamContainer<T>
    {
        [JsonProperty("table")]
        public string Table { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("data")]
        public T[] Data { get; set; }
    }
}