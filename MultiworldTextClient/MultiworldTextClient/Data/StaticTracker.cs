using Newtonsoft.Json;

namespace MultiworldTextClient.Data;

public class StaticTracker
{
    [JsonProperty("datapackage")]
    public Dictionary<string, DatapackageChecksum> Datapackages { get; set; } = new Dictionary<string, DatapackageChecksum>();
    [JsonProperty("player_locations_total")]
    public List<PlayerTotalLocations> PlayerTotalLocations { get; set; } = new List<PlayerTotalLocations>();
}

public class DatapackageChecksum
{
    [JsonProperty("checksum")]
    public string Checksum { get; set; } = "";
}

public class PlayerTotalLocations
{
    [JsonProperty("player")]
    public int Player { get; set; }
    [JsonProperty("total_locations")]
    public int TotalLocations { get; set; }
}