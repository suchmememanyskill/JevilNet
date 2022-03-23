using Newtonsoft.Json;

namespace JevilNet.Services.ImageRequest.Sources.Cade;

public class TheCatApiResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("url")]
    public string Url { get; set; }

    public ImageResponse ToImageResponse() => new(Id, "TheCatApi", Url, "None");
}