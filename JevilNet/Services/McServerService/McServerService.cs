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

    public async Task<StatusGet> GetStatus() => await StatusGet.Get();

    public async Task SetState(bool state)
    {
        StatusGet status = await GetStatus();
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

        await StatusPut.Post(state);
    }

    public async Task<VersionsGet> SetVersion(string version)
    {
        if (Versions.All(x => x.Version != version))
            throw new Exception("Version does not exist on server");
        
        var status = await GetStatus();
        if (!status.IsOffline)
            throw new Exception("Server needs to be offline to set a new version");
                
        await VersionsPut.Post(version);
        return Versions.Find(x => x.Version == version)!;
    }

    public async Task<MapsGet> SetMap(string map)
    {
        if (Maps.All(x => x.Name != map))
            throw new Exception("Map does not exist on server");

        var status = await GetStatus();
        if (!status.IsOffline)
            throw new Exception("Server needs to be offline to set a new map");
        
        await MapsPut.Post(map);
        return Maps.Find(x => x.Name == map)!;
    }

    public async Task CreateMap(string name, string version)
    {
        if (Versions.All(x => x.Version != version))
            throw new Exception("Version does not exist on server");

        await MapsNewPost.Post(name, version);
        await Reload();
    }

    public async Task UploadMap(string mapName, string mcVersion, string url, bool readOnly = false)
    {
        if (!url.EndsWith(".zip"))
            throw new Exception("File does not end with .zip");
        
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        await MapsUploadPost.Post(mapName, mcVersion, await response.Content.ReadAsStreamAsync(), readOnly);
        await Reload();
    }

    public record MapChangeResponse(MapsGet Map, VersionsGet OldVersion, VersionsGet NewVersion);
    public async Task<MapChangeResponse> ChangeMapVersion(string mapName, string newVersion)
    {
        MapsGet? map = Maps.Find(x => x.Name == mapName);
        if (map == null)
            throw new Exception("Map not found");

        VersionsGet? oldVersion = Versions.Find(x => x.Version == map.MinecraftVersion);
        if (oldVersion == null)
            throw new Exception("Old version not found? Please reload");

        VersionsGet? newVer = Versions.Find(x => x.Version == newVersion);
        if (newVer == null)
            throw new Exception("New version not found");

        if (oldVersion == newVer)
            throw new Exception($"Map is already set to use version {oldVersion.Version}");

        await MapsNameVersionPut.Put(map.Name, newVer.Version);
        await Reload();
        return new(map, oldVersion, newVer);
    }

    public async Task DeleteMap(string mapName)
    {
        MapsGet? map = Maps.Find(x => x.Name == mapName);
        if (map == null)
            throw new Exception("Map not found");

        await MapsDelete.Delete(map.Name);
        await Reload();
    }

    private async void SetMcStatus(object? obj)
    {
        try
        {
            var status = await GetStatus();

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
        catch { }
    }
}