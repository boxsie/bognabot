using Bognabot.Data.Exchange.Enums;
using Newtonsoft.Json;

namespace Bognabot.Data.Exchange
{
    public class CommandRequest
    {
        [JsonIgnore]
        public HttpMethod HttpMethod { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public bool IsAuth { get; set; }
    }
}