using Discord;
using Discord.WebSocket;

namespace JevilNet.Services;

public class EmoteService
{
    private DiscordSocketClient _client;
    private Dictionary<ulong, List<GuildEmote>> cachedEmotes = new();
    public List<GuildEmote> CachedEmotesList { get; private set; } = new();
    
    public EmoteService(DiscordSocketClient client)
    {
        _client = client;
        client.GuildUpdated += GuildUpdated;
        client.Ready += async () =>
        {
            foreach (var clientGuild in client.Guilds)
            {
                await GetGuildEmotes(clientGuild);
            }
        };
    }

    public async Task GuildUpdated(SocketGuild before, SocketGuild after)
    {
        List<string> afterEmotes = after.Emotes.Select(y => y.Name).ToList();
        if (before.Emotes.Count != after.Emotes.Count || !before.Emotes.Select(x => x.Name).All(x => afterEmotes.Contains(x)))
        {
            await GetGuildEmotes(after);
        }
    }

    public async Task GetGuildEmotes(SocketGuild guild)
    {
        cachedEmotes[guild.Id] = guild.Emotes.ToList();
        CachedEmotesList = CachedEmotesAsList();
    }

    public GuildEmote? FindEmote(string name) => CachedEmotesList.Find(x => x.Name.ToLower() == name.ToLower());

    private List<GuildEmote> CachedEmotesAsList()
    {
        List<GuildEmote> emotes = new();
        foreach (var cachedEmotesValue in cachedEmotes.Values)
            emotes.AddRange(cachedEmotesValue);

        return emotes;
    }
}