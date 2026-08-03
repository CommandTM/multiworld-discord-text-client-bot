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
        Console.WriteLine("Getting Tracker...");
        var tracker = await GetTracker();
        Console.WriteLine("Got Tracker");

        using (var context = new ItemsDbContext())
        {
            try
            {
                var processedItems = context.ProcessedItems.Where(pi => pi.TrackerUuid.Equals(_trackerUuid)).ToList();
                var itemsToSend = new List<List<long>>();
                Console.WriteLine("Starting Processing Received Items...");
                foreach (var ItemsReceived in tracker.PlayerItemsRecevied)
                {
                    Console.WriteLine($"Starting Processing {ItemsReceived.Player}'s Items...");
                    foreach (var item in ItemsReceived.Items)
                    {
                        if (!processedItems.Any(pi => pi.ItemId == item[0] && pi.LocationId == item[1]))
                        {
                            Console.WriteLine($"Processing item {item[0]} at location {item[1]}...");
                            var classification = item[3];

                            if ((classification & 1) == 1)
                            {
                                item.Add(ItemsReceived.Player);
                                itemsToSend.Add(item);
                            }
                            else
                            {
                                ProcessedItem newProcessedItem = new()
                                {
                                    TrackerUuid = _trackerUuid,
                                    ItemId = item[0],
                                    LocationId = item[1]
                                };
                                context.Add(newProcessedItem);
                            }
                            Console.WriteLine("Done Processing Item");
                        }
                    }

                    Console.WriteLine("Done Processing Player's Items");
                }

                Console.WriteLine("Done Processing Received Items");

                if (itemsToSend.Any())
                {
                    Console.WriteLine("Sending Messages...");
                    string message = "```ansi\n";
                    int maxLength = 1925;
                    foreach (var item in itemsToSend)
                    {
                        Console.WriteLine($"Processing message for item {item[0]} at location {item[1]}...");
                        string receiver = _roomStatus.GetPlayerNameFromId(item[4]);
                        string receiverChecksum =
                            _staticTracker.GetChecksumFromGameName(_roomStatus.GetPlayerGameFromId(item[4]));

                        string sender = _roomStatus.GetPlayerNameFromId(item[2]);
                        string senderChecksum =
                            _staticTracker.GetChecksumFromGameName(_roomStatus.GetPlayerGameFromId(item[2]));

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
                            Console.WriteLine("Sent Message!");
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
                        Console.WriteLine("Done Processing Item Message");
                    }

                    message += "\n```";
                    await Program.SendMessage(message, guildId, channelId);
                    Console.WriteLine("Sent Message!");
                    Console.WriteLine("Done Sending Messages");
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled Exception While Sending Messages: {ex.Message}");
            }
        }
    }

    public async Task SendGoaledSlotsForPercentage(double percentage)
    {
        var tracker = await GetTracker();

        var goaledSlots = tracker.PlayerStatus.Where(ps => ps.Status == 30).Select(ps => ps.Player);
        
        List<int> toRelease =  new List<int>();

        foreach (var goaledSlot in goaledSlots)
        {
            var totalLocations = _staticTracker.GetPlayersTotalLocations(goaledSlot) ?? 1;
            var completedLocations = tracker.PlayerChecksDone.FirstOrDefault(pcd => pcd.Player == goaledSlot).Locations.Count();
            
            if (totalLocations == completedLocations) continue;

            if (percentage <= ((double)completedLocations / (double)totalLocations))
            {
                toRelease.Add(goaledSlot);
            }
        }

        await GetRoomStatus();
        var connectionManager = new MultiworldConnectionManager(_baseUri.Replace("/api", string.Empty).Replace("https://", string.Empty).Replace("http://", String.Empty), _roomStatus.GetPort().ToString());
        foreach (var player in toRelease)
        {
            var slotName = _roomStatus.GetPlayerNameFromId(player);
            var gameName = _roomStatus.GetPlayerGameFromId(player);
            
            connectionManager.ReleaseSlot(gameName, slotName);
        }
    }

    public async Task SendLocation(ulong guildId, ulong channelId, string slotName, string locationName)
    {
        string? gameName = _roomStatus.GetPlayerGameFromPlayerName(slotName);

        if (gameName == null)
        {
            await Program.SendMessage("Not A Valid Slot", guildId, channelId);
            return;
        }
        
        var checksum = _staticTracker.GetChecksumFromGameName(gameName);
        long? locationId = _staticTracker.GetLocationIdFromName(locationName, checksum);
        if (locationId == null)
        {
            await Program.SendMessage("Not A Valid Location", guildId, channelId);
            return;
        }
        
        await GetRoomStatus();
        
        var connectionManager = new MultiworldConnectionManager(_baseUri.Replace("/api", string.Empty).Replace("https://", string.Empty).Replace("http://", String.Empty), _roomStatus.GetPort().ToString());
        try
        {
            connectionManager.SendLocation(gameName, slotName, locationId ?? 0);
        }
        catch
        {
            await Program.SendMessage("Failed To Connect To Room", guildId, channelId);
            return;
        }
            
        await Program.SendMessage("Sent!", guildId, channelId);
    }
    
    public async Task ReleaseSlot(ulong guildId, ulong channelId, string slotName)
    {
        string? gameName = _roomStatus.GetPlayerGameFromPlayerName(slotName);

        if (gameName == null)
        {
            await Program.SendMessage("Not A Valid Slot", guildId, channelId);
            return;
        }
        
        await GetRoomStatus();
        
        var connectionManager = new MultiworldConnectionManager(_baseUri.Replace("/api", string.Empty).Replace("https://", string.Empty).Replace("http://", String.Empty), _roomStatus.GetPort().ToString());
        try
        {
            connectionManager.ReleaseSlot(gameName, slotName);
        }
        catch
        {
            await Program.SendMessage("Failed To Connect To Room", guildId, channelId);
            return;
        }
            
        await Program.SendMessage("Released!", guildId, channelId);
    }

    public async Task CheckReleaseSlot(ulong guildId, ulong channelId, string slotName, double? percentage)
    {
        try
        {
            var playerId = _roomStatus.GetPlayerIdFromPlayerName(slotName);
            if (playerId == null)
            {
                await Program.SendMessage("Not A Valid Slot", guildId, channelId);
                return;
            }
        
            var tracker = await GetTracker();

            if (tracker.PlayerStatus.FirstOrDefault(ps => ps.Player == playerId).Status != 30)
            {
                await Program.SendMessage("Slot Is Not Goaled", guildId, channelId);
                return;
            }
        
            var totalLocations = _staticTracker.GetPlayersTotalLocations((int)playerId) ?? 1;
            var completedLocations = tracker.PlayerChecksDone.FirstOrDefault(pcd => pcd.Player == playerId).Locations.Count();

            if (totalLocations == completedLocations)
            {
                await Program.SendMessage("Slot is already released", guildId, channelId);
                return;
            }

            if (percentage != null && percentage > ((double)completedLocations / (double)totalLocations))
            {
                await Program.SendMessage("Slot does not meet release requirements",  guildId, channelId);
                return;
            }
        
            await ReleaseSlot(guildId, channelId, slotName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled Exception While Checking Release Slot: {ex.Message}");
        }
    }
}