using JevilNet.Services.ImageRequest.Utils;
using Newtonsoft.Json;

namespace JevilNet.Services.ImageRequest.Sources.E6;

public class E6ImageSource : IImageRequest
{
    public List<string> Tags = new();
    public List<string> RequestParams = new();
    public int Limit = 10;
    public int Page = 1;
    public string Sort = "random";

    private string username;
    private string password;

    public E6ImageSource()
    {
        string file = File.ReadAllText("./Storage/e6creds.txt");
        username = file.Split(":")[0].Trim();
        password = file.Split(":")[1].Trim();
    }
    
    public static List<string> SortTypes = new()
    {
        "random", "favcount", "score", "none"
    };
    
    private static Dictionary<string, Action<string, E6ImageSource>> parameterConverters = new()
    {
        { "sort", (s, e) =>
        {
            if (SortTypes.Contains(s))
                e.Sort = s;
            else
                throw new Exception("I do not know this sort type");
        }},
        { "limit", (s, e) =>
        {
            if (int.TryParse(s, out int i))
            {
                if (i is > 300 or < 1)
                    throw new Exception("Limit is smaller than 1 or larger than 300");

                e.Limit = i;
            }
            else
                throw new Exception("Limit is not a number");
        }},
        { "page", (s, e) =>
        {
            if (int.TryParse(s, out int i))
            {
                if (i < 1)
                    throw new Exception("Page is smaller than 1");

                e.Page = i;
            }
            else
                throw new Exception("Page is not a number");
        }}
    };

    public string GetHelp() => throw new NotImplementedException();
    
    public async Task<List<ImageResponse>> GetImages(List<string> searchTags, List<ImageRequestParameter> parameters)
    {
        Tags = searchTags;

        parameters.ForEach(x =>
        {
            if (!parameterConverters.ContainsKey(x.Parameter))
                throw new Exception("Image parameter is invalid");

            parameterConverters[x.Parameter].Invoke(x.Value, this);
        });
        
        RequestParams = new()
        {
            $"limit={Limit}",
            $"page={Page}",
        };

        if (Sort != "none")
            Tags.Add($"order:{Sort}");
        
        string response = await MakeRequest();
        E6RawResponse responses = JsonConvert.DeserializeObject<E6RawResponse>(response)!;
        return responses.Posts.Select(x => x.ToImageResponse()).ToList();
    }
    
    private async Task<string> MakeRequest()
    {
        string url = $"https://e621.net/posts.json?{string.Join("&", RequestParams)}";

        if (Tags.Count > 0)
            url += $"&tags={string.Join(" ", Tags)}";
        
        ProxySender proxySender = ProxySender.GetDefault();

        for (int i = 0; i < 3; i++)
        {
            string? res = await proxySender.Get(new Uri(url), new(username, password));
            if (res != null)
                return res!;
        }

        throw new Exception("Failed to make web request");
    }
}