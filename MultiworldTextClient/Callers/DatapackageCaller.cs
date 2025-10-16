using MultiworldTextClient.Data;
using Newtonsoft.Json;

namespace MultiworldTextClient;

public class DatapackageCaller
{
    private readonly string _endpoint = "/datapackage";
    private string _baseUri;
    private HttpClient _client;
    
    private string _uri => $"{_baseUri}/{_endpoint}/";
    

    public DatapackageCaller(string baseUri)
    {
        _baseUri = baseUri;
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_uri);
    }

    public async Task<Datapackage?> GetDatapackage(string checksum)
    {
        var response = _client.GetAsync(checksum);
        
        var json = await response.Result.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Datapackage>(json);
    }
}