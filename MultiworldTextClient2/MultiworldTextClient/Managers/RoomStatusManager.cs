using MultiworldTextClient.Data;
using Newtonsoft.Json;

namespace MultiworldTextClient.Managers;

public class RoomStatusManager
{
    private readonly string _endpoint = "/room_status";
    private string _baseUri;
    private string _roomUuid;
    private HttpClient _client;
    private RoomStatus _roomStatus;
    
    private string _uri => $"{_baseUri}/{_endpoint}/";
    
    public RoomStatusManager(string baseUri, string roomUuid)
    {
        _baseUri = baseUri;
        _roomUuid = roomUuid;
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_uri);
    }

    public async Task<bool> GetRoomStatusAsync()
    {
        var response = await _client.GetAsync(_roomUuid);
        var json = await response.Content.ReadAsStringAsync();

        _roomStatus = JsonConvert.DeserializeObject<RoomStatus>(json);

        return _roomStatus != null;
    }

    public string GetPlayerNameFromId(long id)
    {
        int index = Convert.ToInt32(id);
        index--;
        
        return _roomStatus.Players[index][0];
    }
    
    public string GetPlayerGameFromId(long id)
    {
        int index = Convert.ToInt32(id);
        index--;
        
        return _roomStatus.Players[index][1];
    }
}