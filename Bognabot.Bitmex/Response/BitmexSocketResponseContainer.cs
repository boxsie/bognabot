using Newtonsoft.Json;

namespace Bognabot.Bitmex.Response
{
    public class BitmexSocketResponseContainer<T>
    {
        [JsonProperty("table")]
        public string Table { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("data")]
        public T[] Data { get; set; }
    }
}