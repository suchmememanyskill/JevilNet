namespace JevilNet.Services.Gift;

public class GiftCarrier
{
    public string GameName;
    public string GameText;
    public long GameId;
    public GiftType GiftType;
    public List<GiftUserStorage> Gifts = new();
    public List<GiftUser> Users = new();

    public GiftCarrier(string gameName, string gameText, long gameId, GiftType giftType)
    {
        GameName = gameName;
        GameText = gameText;
        GameId = gameId;
        GiftType = giftType;
    }
}