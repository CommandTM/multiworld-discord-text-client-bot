using MultiworldTextClient.Data;
using Newtonsoft.Json;

namespace MultiworldTextClient;

public class DatapackageManager
{
    private readonly string _endpoint = "/datapackage";
    private string _baseUri;
    private string _checksum;
    private HttpClient _client;
    private Datapackage? _datapackage;
    
    private string _uri => $"{_baseUri}/{_endpoint}/";
    

    public DatapackageManager(string baseUri, string checksum)
    {
        _baseUri = baseUri;
        _checksum = checksum;
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_uri);
    }

    public async Task<bool> GetDatapackage()
    {
        var response = _client.GetAsync(_checksum);
        
        var json = await response.Result.Content.ReadAsStringAsync();
        _datapackage = JsonConvert.DeserializeObject<Datapackage>(json);
        
        return _datapackage != null;
    }

    public string GetItemNameFromId(long id)
    {
        if (_datapackage == null)
            return string.Empty;
        
        var keys = _datapackage.ItemNameToId.Keys;
        
        var name = keys.FirstOrDefault(k => _datapackage.ItemNameToId[k].Equals(id));
        
        return name ?? string.Empty;
    }
    
    public string GetLocationNameFromId(long id)
    {
        if (_datapackage == null)
            return string.Empty;
        
        var keys = _datapackage.LocationNameToId.Keys;
        
        var name = keys.FirstOrDefault(k => _datapackage.LocationNameToId[k].Equals(id));
        
        return name ?? string.Empty;
    }
}