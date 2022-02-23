namespace JevilNet.Services.Quote.Models;

public class UserQuotes
{
    public ulong UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Quotes { get; set; } = new();
    
    public UserQuotes() {}

    public UserQuotes(ulong userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}