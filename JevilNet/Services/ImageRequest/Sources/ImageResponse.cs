namespace JevilNet.Services.ImageRequest.Sources;

public class ImageResponse
{
    public string Id { get; set; }
    public string Source { get; set; }
    public string Url { get; set; }
    public string Rating { get; set; }

    public ImageResponse()
    {
    }

    public ImageResponse(string id, string source, string url, string rating)
    {
        Id = id;
        Source = source;
        Url = url;
        Rating = rating;
    }
}