using Discord.WebSocket;
using JevilNet.Services.Model;

namespace JevilNet.Services;

public class McServerService
{
    public List<VersionsGet> Versions { get; set; } = new();
    public List<MapsGet> Maps { get; set; } = new();

    public McServerService(DiscordSocketClient client)
    {
        client.Ready += Reload;
    }

    public async Task Reload()
    {
        Versions = await VersionsGet.Get();
        Maps = await MapsGet.Get();
    }

    public async Task<ConfigGet> GetConfig() => await ConfigGet.Get();

    public async Task SetState(bool state)
    {
        ConfigGet config = await GetConfig();
        if (state)
        {
            if (!(config.TextStatus is "Stopped" or "Dead"))
                throw new Exception("Minecraft server is already running!");
        }
        else
        {
            if (config.TextStatus != "Ready")
                throw new Exception("Minecraft server is not ready yet, or not running");
        }

        await ConfigPost.Post(state);
    }

    public async Task<VersionsGet> SetVersion(string version)
    {
        if (Versions.All(x => x.Version != version))
            throw new Exception("Version does not exist on server");

        await VersionsPost.Post(version);
        return Versions.Find(x => x.Version == version)!;
    }

    public async Task<MapsGet> SetMap(string map)
    {
        if (Maps.All(x => x.Name != map))
            throw new Exception("Map does not exist on server");

        await MapsPost.Post(map);
        return Maps.Find(x => x.Name == map)!;
    }

    public async Task CreateMap(string name, string version)
    {
        if (Versions.All(x => x.Version != version))
            throw new Exception("Version does not exist on server");

        await MapsNewPost.Post(name, version);
    }
}