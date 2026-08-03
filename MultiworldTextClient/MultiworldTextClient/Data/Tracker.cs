using Newtonsoft.Json;

namespace MultiworldTextClient.Data;

public class Tracker
{
    [JsonProperty("player_items_received")]
    public List<TrackerReceivedItems> PlayerItemsRecevied { get; set; } = new List<TrackerReceivedItems>();
    [JsonProperty("player_checks_done")]
    public List<TrackerChecksDone> PlayerChecksDone { get; set; } = new List<TrackerChecksDone>();
    [JsonProperty("player_status")]
    public List<TrackerStatus> PlayerStatus { get; set; } = new List<TrackerStatus>();
}

public class TrackerReceivedItems
{
    [JsonProperty("player")]
    public int Player { get; set; } = 0;
    
    [JsonProperty("items")]
    public List<List<long>> Items { get; set; } = new List<List<long>>();
}

public class TrackerChecksDone
{
    [JsonProperty("player")]
    public int Player { get; set; }
    [JsonProperty("locations")]
    public List<long> Locations { get; set; } = new List<long>();
}

public class TrackerStatus
{
    [JsonProperty("player")]
    public int Player { get; set; }
    [JsonProperty("status")]
    public int Status { get; set; }
}