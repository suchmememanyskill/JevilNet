using Discord;
using Discord.Rest;
using Discord.WebSocket;
using JevilNet.Services.UserSpecificGuildStorage;

namespace JevilNet.Services.Roles;

public class RoleService : UserSpecificGuildStorage<Empty, RoleSet>
{
    public override string StoragePath() => "./Storage/roles.json";

    public RoleService(DiscordSocketClient client)
    {
        Load();
    }

    public async Task CreateSet(ulong serverId, ulong userId, string username, string setName)
    {
        if (setName.Length > 50)
            throw new Exception("Role set name is too long!");

        if (GetOrDefaultServerStorage(serverId).GetCombinedStorage().Count >= 20)
            throw new Exception("Server has reached the max amount of role sets");
        
        await AddToUser(serverId, userId, username, new RoleSet(setName));
    }

    public async Task DeleteSet(ulong serverId, ulong userId, int idx)
        => await DelFromUser(serverId, userId, idx);

    public async Task DeleteSet(ulong serverId, ulong userId, RoleSet set)
        => await DelFromUser(serverId, userId, set);

    public RoleSet? GetRoleSet(ulong serverId, ulong userId, string setName)
        => GetOrDefaultUserStorage(serverId, userId).CustomStorage.Find(x => x.SetName == setName);

    public RoleSet? GetRoleSet(ulong serverId, ulong userId, int setId)
        => GetOrDefaultUserStorage(serverId, userId).CustomStorage.Find(x => x.Id == setId);

    public async Task AddToSet(SocketGuild guild, RoleSet set, string name, string desc)
    {
        if (set.Roles.Any(x => x.Name == name))
            throw new Exception("Role already added to list");

        if (name.Length > 100)
            throw new Exception("Role name is too long");
        
        if (desc.Length > 100)
            throw new Exception("Role description is too long");
            
        RestRole role = await guild.CreateRoleAsync(name, GuildPermissions.None, Color.Default, false, false);
        await AddToSet(set, role, desc);
    }

    public async Task AddToSet(RoleSet set, IRole role, string desc)
    {
        if (set.Roles.Any(x => x.Id == role.Id))
            throw new Exception("Role already added to list");
        
        if (desc.Length > 100)
            throw new Exception("Role description is too long");
        
        set.Roles.Add(new RoleEntry(role.Name, desc, role.Id));
        await Save();
    }

    public async Task RemoveFromSet(RoleSet set, ulong roleId)
    {
        if (set.Roles.RemoveAll(x => x.Id == roleId) > 0)
            await Save();
        else
            throw new Exception("Role not found in set");
    }
}