using Discord;
using Discord.WebSocket;
using JevilNet.Services.Model;

namespace JevilNet.Services;

public class McServerService
{
    public List<VersionsGet> Versions { get; set; } = new();
    public List<MapsGet> Maps { get; set; } = new();
    private DiscordSocketClient _client;
    private Timer _timer;

    public McServerService(DiscordSocketClient client)
    {
        _client = client;
        client.Ready += Reload;
        _timer = new(SetMcStatus, null, 60 * 1000, 5 * 60 * 1000);
    }

    public async Task Reload()
    {
        Versions = await VersionsGet.Get();
        Maps = await MapsGet.Get();
    }

    public async Task<StatusGet> GetConfig() => await StatusGet.Get();

    public async Task SetState(bool state)
    {
        StatusGet status = await GetConfig();
        if (state)
        {
            if (!(status.TextStatus is "Stopped" or "Dead"))
                throw new Exception("Minecraft server is already running!");
        }
        else
        {
            if (status.TextStatus != "Ready")
                throw new Exception("Minecraft server is not ready yet, or not running");
        }

        await StatusPost.Post(state);
    }

    public async Task<VersionsGet> SetVersion(string version)
    {
        if (Versions.All(x => x.Version != version))
            throw new Exception("Version does not exist on server");
        
        var status = await GetConfig();
        if (!status.IsOffline)
            throw new Exception("Server needs to be offline to set a new version");
                
        await VersionsPost.Post(version);
        return Versions.Find(x => x.Version == version)!;
    }

    public async Task<MapsGet> SetMap(string map)
    {
        if (Maps.All(x => x.Name != map))
            throw new Exception("Map does not exist on server");

        var status = await GetConfig();
        if (!status.IsOffline)
            throw new Exception("Server needs to be offline to set a new map");
        
        await MapsPost.Post(map);
        return Maps.Find(x => x.Name == map)!;
    }

    public async Task CreateMap(string name, string version)
    {
        if (Versions.All(x => x.Version != version))
            throw new Exception("Version does not exist on server");

        await MapsNewPost.Post(name, version);
    }

    public async Task UploadMap(string mapName, string mcVersion, string url, bool readOnly = false)
    {
        if (!url.EndsWith(".zip"))
            throw new Exception("File does not end with .zip");
        
        using var httpClient = new HttpClient();
        // Please don't steal this url. I pay for this and am too lazy to add auth
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        await MapsUploadPost.Post(mapName, mcVersion, await response.Content.ReadAsStreamAsync(), readOnly);
    }

    private async void SetMcStatus(object? obj)
    {
        var status = await GetConfig();

        if (status.TextStatus == "Ready")
        {
            string map = (status.Map == null || (!status.Version?.UsesMaps ?? false))
                ? ""
                : $" on Map {status.Map.Name} ";

            string version = (status.Version == null) ? "" : $" on Version {status.Version.Version}";
            
            await _client.SetGameAsync($"MC: {status.OnlinePlayers.Count} playing{map}{version}");
        }
        else
        {
            if (_client.Activity is { Type: ActivityType.Playing } && _client.Activity.Name.StartsWith("MC:"))
                await _client.SetGameAsync("");
        }
    }
}