using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;

namespace MultiworldTextClient.Managers;

public class MultiworldConnectionManager
{
    private ArchipelagoSession _session;

    public MultiworldConnectionManager(string baseUrl, string port)
    {
        _session = ArchipelagoSessionFactory.CreateSession($"{baseUrl}:{port}");
    }

    public void SendLocation(string GameName, string SlotName, long locationId)
    {
        var result = _session.TryConnectAndLogin(
            GameName,
            SlotName,
            ItemsHandlingFlags.AllItems,
            new Version(0, 6, 7),
            ["MULTIWORLD DISCORD TEXT CLIENT BOT"],
            requestSlotData: false
        );

        if (!result.Successful)
        {
            throw new Exception("Could not connect to server");
        }
        
        _session.Locations.CompleteLocationChecks(locationId);

        _session.Socket.DisconnectAsync();
    }

    public void ReleaseSlot(string GameName, string SlotName)
    {
        var result = _session.TryConnectAndLogin(
            GameName,
            SlotName,
            ItemsHandlingFlags.AllItems,
            new Version(0, 6, 7),
            ["MULTIWORLD DISCORD TEXT CLIENT BOT"],
            requestSlotData: false
        );

        if (!result.Successful)
        {
            throw new Exception("Could not connect to server");
        }

        var unsent_locations = _session.Locations.AllMissingLocations.ToArray();
        
        _session.SetGoalAchieved();

        if (_session.RoomState.ReleasePermissions != Permissions.Auto)
        {
            _session.Locations.CompleteLocationChecks(unsent_locations);
        }
        
        _session.Socket.DisconnectAsync();
    }
}