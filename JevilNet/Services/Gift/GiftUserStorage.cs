using Newtonsoft.Json;

namespace JevilNet.Services.Gift;

public enum GiftType
{
    Custom = 0,
    Steam,
}

public class GiftUserStorage
{
    public long Id { get; set; } = Program.Random.Next();
    public long GameId { get; set; } = Program.Random.Next();
    [JsonIgnore]
    public GiftType Type { get => (GiftType) TypeInt; set => TypeInt = (int) value; }
    public int TypeInt { get; set; }
    public string GameName { get; set; }
    public string GameKey { get; set; }

    public string GetProperGameText()
    {
        if (Type == GiftType.Steam)
        {
            return $"https://store.steampowered.com/app/{GameId}";
        }

        return GameName;
    }

    public GiftUserStorage()
    {
    }

    public GiftUserStorage(GiftType type, string gameName, string gameKey)
    {
        Type = type;
        GameName = gameName;
        GameKey = gameKey;
    }

    public GiftUserStorage(GiftType type, string gameName, string gameKey, long gameId)
        : this(type, gameName, gameKey)
    {
        GameId = gameId;
    }
}