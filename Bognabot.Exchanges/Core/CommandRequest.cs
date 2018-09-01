using Newtonsoft.Json;

namespace Bognabot.Exchanges.Core
{
    public class CommandRequest
    {
        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public bool IsAuth { get; set; }
    }
}