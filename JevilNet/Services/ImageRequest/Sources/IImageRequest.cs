namespace JevilNet.Services.ImageRequest.Sources;

public record ImageRequestParameter(string Parameter, string Value);

public interface IImageRequest
{
    string GetHelp();
    Task<List<ImageResponse>> GetImages(List<string> searchTags, List<ImageRequestParameter> parameters);
}