using Newtonsoft.Json;

namespace MultiworldTextClient.Data;

public class RoomStatus
{
    [JsonProperty("players")]
    public List<List<string>> Players { get; set; } = new List<List<string>>();
}