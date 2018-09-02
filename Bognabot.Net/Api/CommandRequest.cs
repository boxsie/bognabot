using Newtonsoft.Json;

namespace Bognabot.Net.Api
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