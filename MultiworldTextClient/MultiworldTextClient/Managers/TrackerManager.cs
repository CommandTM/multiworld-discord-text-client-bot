using MultiworldTextClient.Data;
using MultiworldTextClient.Data.Database;
using Newtonsoft.Json;

namespace MultiworldTextClient.Managers;

public class TrackerManager
{
    private readonly string _endpoint = "/tracker";
    private string _baseUri;
    private string _trackerUuid;
    private string _roomUuid;
    private HttpClient _client;
    private StaticTrackerManager _staticTracker;
    private RoomStatusManager _roomStatus;
    
    private string _uri => $"{_baseUri}/{_endpoint}/";
    
    public TrackerManager(string baseUri, string trackerUuid, string roomUudi)
    {
        _baseUri = baseUri;
        _trackerUuid = trackerUuid;
        _roomUuid = roomUudi;
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_uri);
    }

    public async Task<bool> GetStaticTracker()
    {
        _staticTracker = new StaticTrackerManager(_baseUri, _trackerUuid);
        bool worked = await _staticTracker.GetStaticTracker();
        if (!worked)
            return worked;
        
        worked = await _staticTracker.PopulateDatapackages();
        return worked;
    }

    public async Task<bool> GetRoomStatus()
    { 
        _roomStatus = new RoomStatusManager(_baseUri, _roomUuid);
        return await _roomStatus.GetRoomStatusAsync();
    }

    private async Task<Tracker> GetTracker()
    {
        var response = await _client.GetAsync(_trackerUuid);
        var json = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<Tracker>(json);
    }

    public async Task SendItemMessaages()
    {
        var tracker = await GetTracker();

        using (var context = new ItemsDbContext())
        {
            var processedItems = context.ProcessedItems.Where(pi => pi.TrackerUuid.Equals(_trackerUuid)).ToList();
            foreach (var ItemsReceived in tracker.PlayerItemsRecevied)
            {
                string receiver = _roomStatus.GetPlayerNameFromId(ItemsReceived.Player);
                string receiverChecksum = _staticTracker.GetChecksumFromGameName(_roomStatus.GetPlayerGameFromId(ItemsReceived.Player));
                foreach (var Item in ItemsReceived.Items)
                {
                    if (!processedItems.Any(pi => pi.ItemId == Item[0] && pi.LocationId == Item[1]))
                    {
                        var classification = Item[3];

                        if ((classification & 1) == 1)
                        {
                            string sender = _roomStatus.GetPlayerNameFromId(Item[2]);
                            string senderChecksum = _staticTracker.GetChecksumFromGameName(_roomStatus.GetPlayerGameFromId(Item[2]));

                            string item = _staticTracker.GetItemNameFromId(Item[0], receiverChecksum);
                            string location = _staticTracker.GetLocationNameFromId(Item[1], senderChecksum);
                        
                            Console.WriteLine($"{sender} sent {item} to {receiver} ({location})");
                            ProcessedItem newProcessedItem = new()
                            {
                                TrackerUuid = _trackerUuid,
                                ItemId = Item[0],
                                LocationId = Item[1]
                            };
                            context.Add(newProcessedItem);
                        }
                    }
                }
                context.SaveChanges();
            }
        }
    }
}