using Newtonsoft.Json;

namespace JevilNet.Services.ImageRequest.Sources;

public class R34Response
{
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("file_url")]
    public string Url { get; set; }
    [JsonProperty("rating")]
    public string Rating { get; set; }

    public ImageResponse ToImageResponse() => new(Id.ToString(), "r34", Url, Rating);
}