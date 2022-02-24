namespace JevilNet.Services.UserSpecificGuildStorage;

public class ServerStorage<TCustomServerStorage, TCustomUserStorage> 
    where TCustomServerStorage : new()
{
    public ulong ServerId { get; set; }
    public List<UserStorage<TCustomUserStorage>> UserStorage { get; set; } = new();
    public TCustomServerStorage CustomStorage { get; set; } = new();

    public ServerStorage() {}
    public ServerStorage(ulong serverId) => ServerId = serverId;

    public List<TCustomUserStorage> GetCombinedStorage()
    {
        List<TCustomUserStorage> storage = new();
        UserStorage.ForEach(x => x.CustomStorage.ForEach(y => storage.Add(y)));
        return storage;
    }
}