using Newtonsoft.Json;

namespace MultiworldTextClient.Data;

public class StaticTracker
{
    [JsonProperty("datapackage")]
    public Dictionary<string, DatapackageChecksum> Datapackages { get; set; } = new Dictionary<string, DatapackageChecksum>();
}

public class DatapackageChecksum
{
    [JsonProperty("checksum")]
    public string Checksum { get; set; } = "";
}