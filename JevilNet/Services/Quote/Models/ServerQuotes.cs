namespace JevilNet.Services.Quote.Models;

public class ServerQuotes
{
    public ulong ServerId { get; set; }
    public List<UserQuotes> UserQuotes { get; set; } = new();
    public ulong QuoteChannel { get; set; } = 0;

    public ServerQuotes() {}
    public ServerQuotes(ulong serverId) => ServerId = serverId;

    public List<string> GetCombinedQuotes()
    {
        List<string> quotes = new();
        UserQuotes.ForEach(x => x.Quotes.ForEach(y => quotes.Add(y)));
        return quotes;
    }
}