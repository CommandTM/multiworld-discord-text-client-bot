using Newtonsoft.Json;

namespace MultiworldTextClient.Data;

public class Tracker
{
    [JsonProperty("player_items_received")]
    public List<TrackerReceivedItems> PlayerItemsRecevied { get; set; } = new List<TrackerReceivedItems>();
}

public class TrackerReceivedItems
{
    [JsonProperty("player")]
    public int Player { get; set; } = 0;
    
    [JsonProperty("items")]
    public List<List<long>> Items { get; set; } = new List<List<long>>();
}