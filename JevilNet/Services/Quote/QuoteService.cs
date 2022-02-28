using Discord;
using Discord.WebSocket;
using JevilNet.Services.UserSpecificGuildStorage;

namespace JevilNet.Services.Quote;

public class QuoteService : UserSpecificGuildStorage<ulong, string>
{
    public override string StoragePath() => "./Storage/quote.json";
    private DiscordSocketClient client;
    private Timer timer;

    public QuoteService(DiscordSocketClient client)
    {
        Load();
        this.client = client;
        timer = new(MessageChecker, null, 10 * 60 * 1000, 10 * 60 * 1000);
    }

    public async Task AddQuote(ulong serverId, ulong userId, string username, string quote)
        => await AddToUser(serverId, userId, username, quote);

    public async Task DelQuote(ulong serverId, ulong userId, int idx)
        => await DelFromUser(serverId, userId, idx);
    
    public async Task EditQuote(ulong serverId, ulong userId, int idx, string newQuote)
    {
        var userQuotes = GetOrDefaultUserStorage(serverId, userId);

        if (idx < 0 || idx >= userQuotes.CustomStorage.Count)
            throw new Exception("Index out of range");

        userQuotes.CustomStorage[idx] = newQuote;
        await Save();
    }

    public async Task SetQuoteChannel(ulong serverId, ulong channelId)
        => await SetCustomStorage(serverId, channelId);

    public async Task<string?> GetOldMessage(ITextChannel channel)
    {
        var messages = await channel
            .GetMessagesAsync(SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow.AddMonths(-3)), Direction.Before, 50).FlattenAsync();

        messages = messages.Where(x => !string.IsNullOrWhiteSpace(x.Content));

        var enumerable = messages.ToList();
        int count = enumerable.Count;
        
        return count < 0 ? null : enumerable[Program.Random.Next(count)].Content;
    }

    private void MessageChecker(object state) => MessageCheckerAsync().GetAwaiter().GetResult();

    private async Task MessageCheckerAsync()
    {
        //Console.WriteLine("[Quote] Starting message scanning...");
        foreach (var serverQuotes in storage)
        {
            if (serverQuotes.CustomStorage <= 0)
                continue;

            IChannel channel = await client.GetChannelAsync(serverQuotes.CustomStorage);
            if (channel is ITextChannel textChannel)
            {
                var messages = await textChannel.GetMessagesAsync(1).FlattenAsync();

                if (messages.Count() < 1)
                    continue;

                var message = messages.First();
                
                if (message.Author.IsBot)
                    continue;

                var minutesSinceLastMessage = (DateTime.UtcNow - message.Timestamp).TotalMinutes;
                Console.WriteLine($"[Quote] {serverQuotes.ServerId}'s minutes since last message: {minutesSinceLastMessage}");

                if (minutesSinceLastMessage > 60) // One hour
                {
                    List<string> combinedQuotes = serverQuotes.GetCombinedStorage();
                    
                    if (combinedQuotes.Count > 0 && Program.Random.Next(2) == 1)
                    {
                        string random = combinedQuotes[Program.Random.Next(combinedQuotes.Count)];
                        await textChannel.SendMessageAsync(random, allowedMentions: AllowedMentions.None);
                        continue;
                    }

                    string? oldMessage = await GetOldMessage(textChannel);
                    if (oldMessage != null)
                    {
                        await textChannel.SendMessageAsync(oldMessage, allowedMentions: AllowedMentions.None);
                    }
                }
            }
        }
    }
}