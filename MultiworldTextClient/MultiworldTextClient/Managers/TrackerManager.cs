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

    public async Task SendItemMessaages(ulong guildId, ulong channelId)
    {
        var tracker = await GetTracker();

        using (var context = new ItemsDbContext())
        {
            var processedItems = context.ProcessedItems.Where(pi => pi.TrackerUuid.Equals(_trackerUuid)).ToList();
            var itemsToSend = new List<List<long>>();
            foreach (var ItemsReceived in tracker.PlayerItemsRecevied)
            {
                foreach (var item in ItemsReceived.Items)
                {
                    if (!processedItems.Any(pi => pi.ItemId == item[0] && pi.LocationId == item[1]))
                    {
                        var classification = item[3];

                        if ((classification & 1) == 1)
                        {
                            item.Add(ItemsReceived.Player);
                            itemsToSend.Add(item);
                        }
                    }
                }
            }

            if (itemsToSend.Any())
            {
                string message = "```ansi\n";
                int maxLength = 1925;
                foreach (var item in itemsToSend)
                {
                    string receiver = _roomStatus.GetPlayerNameFromId(item[4]);
                    string receiverChecksum = _staticTracker.GetChecksumFromGameName(_roomStatus.GetPlayerGameFromId(item[4]));
                
                    string sender = _roomStatus.GetPlayerNameFromId(item[2]);
                    string senderChecksum = _staticTracker.GetChecksumFromGameName(_roomStatus.GetPlayerGameFromId(item[2]));

                    string itemName = _staticTracker.GetItemNameFromId(item[0], receiverChecksum);
                    string location = _staticTracker.GetLocationNameFromId(item[1], senderChecksum);

                    string itemMessage =
                        $"[2;33m{sender}[0m [2;37msent[0m [2;35m{itemName}[0m[0;2m[0;2m[0m[0m[2;40m[2;42m[0;2m[0m[2;42m[0m[2;40m[0m [2;37mto[0m [2;33m{receiver}[0m [2;37m([0m[2;37m{location}[0m[2;37m)[0m";
                    if (message.Length + itemMessage.Length < maxLength)
                    {
                        message += itemMessage + "\n";
                    }
                    else
                    {
                        message += "\n```";
                        await Program.SendMessage(message, guildId, channelId);
                        message = "```ansi\n";
                        message += itemMessage + "\n";
                    }
                    ProcessedItem newProcessedItem = new()
                    {
                        TrackerUuid = _trackerUuid,
                        ItemId = item[0],
                        LocationId = item[1]
                    };
                    context.Add(newProcessedItem);
                }

                message += "\n```";
                await Program.SendMessage(message, guildId, channelId);
                context.SaveChanges();
            }
        }
    }
}