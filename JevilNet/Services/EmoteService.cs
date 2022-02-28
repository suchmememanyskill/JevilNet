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
        client.MessageReceived += MessageReceived;
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

    public async Task MessageReceived(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (!(rawMessage is SocketUserMessage message))
            return;
        if (message.Source != MessageSource.User)
            return;
        
        if (message.Content.Contains(':'))
        {
            List<string> split = message.Content.Split(':').ToList();
            bool inTag = false;
            foreach (var x in split)
            {
                if (inTag)
                {
                    if (x.Contains('>') && !x.Contains('<'))
                        inTag = false;
                }
                else
                {
                    if (x.Contains('<'))
                    {
                        if (x.Contains('>'))
                            continue;
                        
                        inTag = true;
                    }
                    else
                    {
                        GuildEmote? emote = FindEmote(x);
                        if (emote != null)
                            await message.AddReactionAsync(emote);
                    }
                }
            }
        }
    }
    
    private List<GuildEmote> CachedEmotesAsList()
    {
        List<GuildEmote> emotes = new();
        foreach (var cachedEmotesValue in cachedEmotes.Values)
            emotes.AddRange(cachedEmotesValue);

        return emotes;
    }
}