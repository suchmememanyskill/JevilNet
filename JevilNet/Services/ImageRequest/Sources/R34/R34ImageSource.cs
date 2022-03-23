using System.Net;
using JevilNet.Services.ImageRequest.Utils;
using Newtonsoft.Json;

namespace JevilNet.Services.ImageRequest.Sources;

public class R34ImageSource : IImageRequest
{
    public List<string> Tags = new();
    public List<string> RequestParams = new();
    public int Limit = 10;
    public int Page = 1;
    public string Sort = "random";
    
    public static List<string> SortTypes = new()
    {
        "id", "score", "rating", "user", "height", "width", "parent", "source", "updated"
    };

    private static Dictionary<string, Action<string, R34ImageSource>> parameterConverters = new()
    {
        { "sort", (s, r) =>
        {
            if (s == "random")
            {
                r.Sort = "random";
                return;
            }

            if (SortTypes.Contains(s))
                r.Sort = s;
            else
                throw new Exception("I do not know this sort type");
        }},
        { "limit", (s, r) =>
        {
            if (int.TryParse(s, out int i))
            {
                if (i is > 1000 or < 1)
                    throw new Exception("Limit is smaller than 1 or larger than 1000");

                r.Limit = i;
            }
            else
                throw new Exception("Limit is not a number");
        }},
        { "page", (s, r) =>
        {
            if (int.TryParse(s, out int i))
            {
                if (i < 1)
                    throw new Exception("Page is smaller than 1");

                r.Page = i;
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
            "page=dapi",
            "s=post",
            "q=index",
            "json=1",
            $"limit={Limit}",
            $"pid={Page}",
        };

        if (Sort != "random")
            Tags.Add("sort:" + Sort);
        else
            return await GetImagesRandom();

        string response = await MakeRequest();
        List<R34Response> responses = JsonConvert.DeserializeObject<List<R34Response>>(response)!;
        return responses.Select(x => x.ToImageResponse()).ToList();
    }

    public async Task<List<ImageResponse>> GetImagesRandom()
    {
        RequestParams = new()
        {
            "page=dapi",
            "s=post",
            "q=index",
            "limit=0"
        };

        string count = await MakeRequest();
        string a = count.Split("count=\"")[1];
        string b = a.Split("\"")[0];
        long actualCount = long.Parse(b);
        int pageCount = (int)((actualCount + 999) / 1000);

        if (pageCount > 200)
            pageCount = 200; // For some reason r34 doesn't allow the api to go past 200 pages??
        
        int page = Program.Random.Next(0, pageCount);
        
        RequestParams = new()
        {
            "page=dapi",
            "s=post",
            "q=index",
            "json=1",
            "limit=1000",
            $"pid={page}",
        };
        
        string response = await MakeRequest();
        List<R34Response> responses = JsonConvert.DeserializeObject<List<R34Response>>(response)!;
        return responses.OrderBy(x => Program.Random.Next()).Take(Limit).Select(x => x.ToImageResponse()).ToList();
    }
    
    private async Task<string> MakeRequest()
    {
        string url = $"https://api.rule34.xxx/index.php?{string.Join("&", RequestParams)}";
        if (Tags.Count > 0)
            url += $"&tags={string.Join(" ", Tags)}";
        
        ProxySender proxySender = ProxySender.GetDefault();

        for (int i = 0; i < 3; i++)
        {
            string? res = await proxySender.Get(new Uri(url));
            if (res != null)
                return res!;
        }

        throw new Exception("Failed to make web request");
    }
}