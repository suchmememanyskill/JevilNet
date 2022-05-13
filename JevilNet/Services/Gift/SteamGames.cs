using Newtonsoft.Json;

namespace JevilNet.Services.Gift;

public class SteamGames
{
    [JsonProperty("applist")]
    public SteamApps AppList { get; set; }
}

public class SteamApps
{
    [JsonProperty("apps")]
    public List<SteamApp> Apps { get; set; }
}

public class SteamApp
{
    [JsonProperty("appid")]
    public long AppId { get; set; }

    [JsonProperty("name")] 
    public string Name { get; set; }
}