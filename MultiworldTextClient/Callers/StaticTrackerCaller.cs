namespace MultiworldTextClient.Data;

public class StaticTrackerCaller
{
    private readonly string _endpoint = "/static_tracker";
    private string _baseUri;
    private string _roomUuid;
    private HttpClient _client;
    
    private string _uri => $"{_baseUri}/{_endpoint}/";

    public StaticTrackerCaller(string baseUri, string roomUuid)
    {
        _baseUri = baseUri;
        _roomUuid = roomUuid;
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_uri);
    }

    public async Task<string> GetStaticTracker()
    {
        var response = _client.GetAsync(_roomUuid);
        
        return await response.Result.Content.ReadAsStringAsync();
    }
}