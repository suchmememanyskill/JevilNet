using Newtonsoft.Json;

namespace JevilNet.Services.ImageRequest.Sources.E6;

public class E6RawResponse
{
    [JsonProperty("posts")]
    public List<E6Response> Posts { get; set; }
}

public class E6Response
{
    [JsonProperty("id")]
    public long Id { get; set; }
    
    [JsonProperty("file")]
    public E6FileResponse File { get; set; }

    public ImageResponse ToImageResponse() => new(Id.ToString(), "E6", File.Url, "mature");
}

public class E6FileResponse
{
    [JsonProperty("url")]
    public string Url { get; set; }
}