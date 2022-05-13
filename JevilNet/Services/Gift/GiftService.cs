using Discord.WebSocket;
using JevilNet.Services.UserSpecificGuildStorage;
using Newtonsoft.Json;

namespace JevilNet.Services.Gift;

public class GiftService : UserSpecificGuildStorage<Empty, GiftUserStorage>
{
    public override string StoragePath() => "./Storage/gifts.json";
    private DiscordSocketClient client;
    private List<SteamApp> steamApps;
    public List<GiftCarrier> cachedGifts { get; private set; }

    public GiftService(DiscordSocketClient client)
    {
        this.client = client;
        Load();
        GetCombinedGifts();
        new Thread(x => GetSteamApps()).Start();
    }

    public async Task AddSteamKey(ulong serverId, ulong userId, string username, long steamId, string key)
    {
        SteamApp? app = steamApps.Find(x => x.AppId == steamId);

        if (app == null)
            throw new Exception("Steam game not found");
        
        await AddToUser(serverId, userId, username, new GiftUserStorage(GiftType.Steam, app.Name, key, app.AppId));
        GetCombinedGifts();
    }

    public async Task AddCustomKey(ulong serverId, ulong userId, string username, string gameName, string key)
    {
        GiftUserStorage gift = new(GiftType.Custom, gameName, key);
        GiftCarrier? carrier = cachedGifts.Find(x =>
            String.Equals(x.GameName, gameName, StringComparison.CurrentCultureIgnoreCase));
        
        if (carrier is {GiftType: GiftType.Custom})
        {
            gift.Id = carrier.GameId;
            gift.GameName = carrier.GameName;
        }

        await AddToUser(serverId, userId, username, gift);
        GetCombinedGifts();
    }
    
    public async Task AddSteamKey(ulong serverId, ulong userId, string username, string steamGameName, string key)
    {
        SteamApp? app = steamApps.Find(x => String.Equals(x.Name, steamGameName, StringComparison.CurrentCultureIgnoreCase));
        
        if (app == null)
            throw new Exception("Steam game not found");
        
        await AddToUser(serverId, userId, username, new GiftUserStorage(GiftType.Steam, app.Name, key, app.AppId));
        GetCombinedGifts();
    }

    public async Task RemoveKey(GiftUserStorage gift)
    {
        foreach (var x in storage)
        {
            foreach (var y in x.UserStorage)
            {
                foreach (var z in y.CustomStorage)
                {
                    if (z == gift)
                    {
                        await DelFromUser(x.ServerId, y.UserId, gift);
                        GetCombinedGifts();
                        return;
                    }
                }
            }
        }
        
        throw new Exception("Did not find server the gift is attached to");
    }

    private List<GiftCarrier> GetCombinedGifts()
    {
        List<GiftCarrier> gifts = new();
        storage.ForEach(x => 
            x.UserStorage.ForEach(y => 
                y.CustomStorage.ForEach(z =>
                {
                    GiftCarrier? carrier = gifts.Find(a => a.GameId == z.GameId);
                    if (carrier == null)
                    {
                        carrier = new(z.GameName, z.GetProperGameText(), z.GameId, z.Type);
                        gifts.Add(carrier);
                    }

                    GiftUser? user = carrier.Users.Find(a => a.UserId == y.UserId);
                    if (user == null)
                    {
                        user = new(y.UserId, y.UserName);
                        carrier.Users.Add(user);
                    }
                    
                    carrier.Gifts.Add(z);
                    user.GiftIds.Add(z.Id);
                })));

        cachedGifts = gifts;
        return gifts;
    }

    private void GetSteamApps()
    {
        using (HttpClient client = new())
        {
            var result = client.GetAsync(new Uri("https://api.steampowered.com/ISteamApps/GetAppList/v2/")).GetAwaiter().GetResult();
            string textResponse = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            SteamGames games = JsonConvert.DeserializeObject<SteamGames>(textResponse);
            steamApps = games.AppList.Apps;
            Console.WriteLine($"Loaded {steamApps.Count} steam games");
        }
    }
}