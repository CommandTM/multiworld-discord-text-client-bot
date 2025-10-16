using Newtonsoft.Json;

namespace MultiworldTextClient.Data;

public class Datapackage
{
    [JsonProperty("checksum")]
    public string Checksum { get; set; } = "";
    
    [JsonProperty("item_name_groups")]
    public Dictionary<string, List<string>> ItemNameGroups { get; set; } =  new Dictionary<string, List<string>>();
    
    [JsonProperty("item_name_to_id")]
    public Dictionary<string, long> ItemNameToId { get; set; } = new Dictionary<string, long>();
    
    [JsonProperty("location_name_groups")]
    public Dictionary<string, List<string>> LocationNameGroups = new Dictionary<string, List<string>>();
    
    [JsonProperty("location_name_to_id")]
    public Dictionary<string, long> LocationNameToId { get; set; } = new Dictionary<string, long>();
}