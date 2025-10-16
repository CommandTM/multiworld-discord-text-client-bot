using MultiworldTextClient.Data;
using Newtonsoft.Json;

namespace MultiworldTextClient;

public class DatapackageProcessor
{
    private readonly string _endpoint = "/datapackage";
    private string _baseUri;
    private string _uri => $"{_baseUri}/{_endpoint}/";
    private HttpClient _client;

    public DatapackageProcessor(string baseUri)
    {
        _baseUri = baseUri;
        _client = new HttpClient();
    }

    public async Task<Datapackage?> GetDatapackage(string checksum)
    {
        _client.BaseAddress = new Uri(_uri);
        var response = _client.GetAsync(checksum);
        
        var json = await response.Result.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Datapackage>(json);
    }
}