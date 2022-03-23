using JevilNet.Services.ImageRequest.Utils;
using Newtonsoft.Json;

namespace JevilNet.Services.ImageRequest.Sources.Cade;

public class TheCatApiSource : IImageRequest
{
    public int limit = 1;
    
    public string GetHelp()
    {
        throw new NotImplementedException();
    }

    private static Dictionary<string, Action<string, TheCatApiSource>> parameterConverters = new()
    {
        {
            "limit", (s, c) =>
            {
                if (int.TryParse(s, out int i))
                {
                    if (i < 1 || i > 10)
                        throw new Exception("Limit is too large!");

                    c.limit = i;
                }
                else
                    throw new Exception("Limit is not a number");
            }
        }
    };

    public async Task<List<ImageResponse>> GetImages(List<string> searchTags, List<ImageRequestParameter> parameters)
    {
        ProxySender proxySender = ProxySender.GetDefault();
        
        parameters.ForEach(x =>
        {
            if (!parameterConverters.ContainsKey(x.Parameter))
                throw new Exception("Image parameter is invalid");

            parameterConverters[x.Parameter].Invoke(x.Value, this);
        });

        string response = await proxySender.Get(new Uri($"https://api.thecatapi.com/v1/images/search?limit={limit}&size=full"));
        List<TheCatApiResponse> responses = JsonConvert.DeserializeObject<List<TheCatApiResponse>>(response)!;

        return responses.Select(x => x.ToImageResponse()).ToList();
    }
}