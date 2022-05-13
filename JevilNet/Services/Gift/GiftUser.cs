namespace JevilNet.Services.Gift;

public class GiftUser
{
    public ulong UserId { get; set; }
    public string UserName { get; set; }

    public List<long> GiftIds { get; set; } = new();

    public GiftUser(ulong userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}