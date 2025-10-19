using Newtonsoft.Json;

namespace MultiworldTextClient.Data;

public class StaticTrackerManager
{
    private readonly string _endpoint = "/static_tracker";
    private string _baseUri;
    private string _trackerUuid;
    private HttpClient _client;
    private StaticTracker? _staticTracker;
    private Dictionary<string, DatapackageManager> _datapackages = new();
    
    private string _uri => $"{_baseUri}/{_endpoint}/";

    public StaticTrackerManager(string baseUri, string trackerUuid)
    {
        _baseUri = baseUri;
        _trackerUuid = trackerUuid;
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_uri);
    }

    public async Task<bool> GetStaticTracker()
    {
        var response = await _client.GetAsync(_trackerUuid);
        
        var json = await response.Content.ReadAsStringAsync();
        _staticTracker = JsonConvert.DeserializeObject<StaticTracker>(json);

        return _staticTracker != null;
    }

    public async Task<bool> PopulateDatapackages()
    {
        if (_staticTracker == null)
            return false;

        foreach (var key in _staticTracker.Datapackages.Keys)
        {
            string checksum = _staticTracker.Datapackages[key].Checksum;
            DatapackageManager datapackage = new DatapackageManager(_baseUri, checksum);
            await datapackage.GetDatapackage();
            
            _datapackages.Add(checksum, datapackage);
        }

        return true;
    }

    public string GetItemNameFromId(long id, string checksum)
    {
        if (!_datapackages.ContainsKey(checksum))
            return string.Empty;

        return _datapackages[checksum].GetItemNameFromId(id);
    }
    
    public string GetLocationNameFromId(long id, string checksum)
    {
        if (!_datapackages.ContainsKey(checksum))
            return string.Empty;

        return _datapackages[checksum].GetLocationNameFromId(id);
    }

    public string GetChecksumFromGameName(string gameName)
    {
        if (!_staticTracker.Datapackages.ContainsKey(gameName))
            return string.Empty;
        
        return _staticTracker.Datapackages[gameName].Checksum;
    }
}