namespace JevilNet.Services.UserSpecificGuildStorage;

public abstract class UserSpecificGuildStorage<TCustomServerStorage, TCustomUserStorage> : BaseService<List<ServerStorage<TCustomServerStorage, TCustomUserStorage>>>
    where TCustomServerStorage : new()
{
    public UserSpecificGuildStorage() => Load();

    public ServerStorage<TCustomServerStorage, TCustomUserStorage> GetOrDefaultServerStorage(ulong serverId)
        => storage.Find(x => x.ServerId == serverId) ?? new(serverId);

    public UserStorage<TCustomUserStorage> GetOrDefaultUserStorage(
        ServerStorage<TCustomServerStorage, TCustomUserStorage> server, ulong userId, string username = "")
        => server.UserStorage.Find(x => x.UserId == userId) ?? new(userId, username);

    public UserStorage<TCustomUserStorage> GetOrDefaultUserStorage(ulong serverId, ulong userId, string username = "")
        => GetOrDefaultUserStorage(GetOrDefaultServerStorage(serverId), userId, username);

    public async Task AddToUser(ulong serverId, ulong userId, string username, TCustomUserStorage obj)
    {
        var serverStorage = GetOrDefaultServerStorage(serverId);
        var userStorage = GetOrDefaultUserStorage(serverStorage, userId, username);
        userStorage.CustomStorage.Add(obj);
        
        if (!serverStorage.UserStorage.Contains(userStorage))
            serverStorage.UserStorage.Add(userStorage);
        
        if (!storage.Contains(serverStorage))
            storage.Add(serverStorage);

        await Save();
    }

    public async Task DelFromUser(ulong serverId, ulong userId, TCustomUserStorage obj)
    {
        var userStorage = GetOrDefaultUserStorage(serverId, userId);
        if (userStorage.CustomStorage.Remove(obj))
            await Save();
    }

    public async Task DelFromUser(ulong serverId, ulong userId, int idx)
    {
        var userStorage = GetOrDefaultUserStorage(serverId, userId);
        if (userStorage.CustomStorage.Count <= idx || idx < 0)
            throw new Exception("Index out of range");

        await DelFromUser(serverId, userId, userStorage.CustomStorage[idx]);
    }

    public async Task DelUser(ulong serverId, ulong userId)
    {
        var serverStorage = GetOrDefaultServerStorage(serverId);
        if (serverStorage.UserStorage.RemoveAll(x => x.UserId == userId) > 0)
            await Save();
    }

    public async Task SetCustomStorage(ulong serverId, TCustomServerStorage customStorage)
    {
        var serverStorage = GetOrDefaultServerStorage(serverId);
        serverStorage.CustomStorage = customStorage;
        
        if (!storage.Contains(serverStorage))
            storage.Add(serverStorage);

        await Save();
    }
}