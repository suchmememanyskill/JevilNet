using Discord;
using Discord.WebSocket;
using JevilNet.Services.Roles;
using JevilNet.Services.UserSpecificGuildStorage;

namespace JevilNet.Modules.Base;

public interface IRoleInterface : IBaseInterface
{
    public RoleService RoleService { get; set; }

    public RoleSet GetCurrentSet(string setName, IUser? user = null)
    {
        if (user == null)
            user = User();

        RoleSet? set = RoleService.GetRoleSet(Guild().Id, user.Id, setName);
        
        if (set == null)
            Exception("Did not find set");

        return set!;
    }
    
    public RoleSet GetCurrentSet(int setId, IUser? user = null)
    {
        if (user == null)
            user = User();

        RoleSet? set = RoleService.GetRoleSet(Guild().Id, user.Id, setId);
        
        if (set == null)
            Exception("Did not find set");

        return set!;
    }
    
    // Get
    public async Task GetRoleSets()
    {
        string combined = string.Join("\n",
            RoleService.GetOrDefaultServerStorage(Guild().Id).GetCombinedStorage().Select(x => x.SetName));

        if (string.IsNullOrWhiteSpace(combined))
            Exception("No role sets found");
        
        await RespondEphermeral($"Role sets in {Guild().Name}:\n\n{combined}");
    }

    public async Task GetRoleSets(ulong userId)
    {
        UserStorage<RoleSet>? user = RoleService.GetOrDefaultServerStorage(Guild().Id).UserStorage
            .Find(x => x.UserId == userId);

        if (user == null || user.CustomStorage.Count <= 0)
            Exception("User has no role sets");

        string combined = string.Join("\n", user.CustomStorage.Select(x => x.SetName));
        await RespondEphermeral($"Role sets in {Guild().Name} from {user.UserName}:\n\n{combined}");
    }
    
    public async Task ViewSet(string setName)
    {
        RoleSet? set = RoleService.GetOrDefaultServerStorage(Guild().Id).GetCombinedStorage()
            .Find(x => x.SetName == setName);

        if (set == null)
            Exception("Did not find set");
        
        await ViewSet(set!.Id);
    }

    public async Task ViewSet(int setId)
    {
        RoleSet? set = RoleService.GetOrDefaultServerStorage(Guild().Id).GetCombinedStorage()
            .Find(x => x.Id == setId);
        
        if (set == null)
            Exception("Did not find set");
        
        if (set.Roles.Count <= 0)
            Exception("Set is empty!");

        var menu = new SelectMenuBuilder()
            .WithPlaceholder("Select roles you want")
            .WithCustomId($"roleselect:{set.Id}")
            .WithMinValues(0)
            .WithMaxValues(set.Roles.Count);
        
        set.Roles.ForEach(x => menu.AddOption(x.Name, x.Id.ToString(), x.Description));

        await RespondEphermeral($"Pick roles from {set.SetName}!", components:  new ComponentBuilder().WithSelectMenu(menu).Build());
    }

    
    // Add
    public async Task AddRoleSet(string setName)
    {
        await RoleService.CreateSet(Guild().Id, User().Id, User().Username, setName);
        await React(Emoji.Parse(":+1:"));
    }
    

    public async Task AddRoleToSet(string setName, string roleName, string description, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setName, user);
        await AddRoleToSet(set.Id, roleName, description, user);
    }

    public async Task AddRoleToSet(int setId, string roleName, string description, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setId, user);
        
        await RoleService.AddToSet(Guild(), set!, roleName, description);
        await React(Emoji.Parse(":+1:"));
    }
    
    public async Task AddRoleToSet(string setName, ulong roleId, string description, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setName, user);
        await AddRoleToSet(set.Id, roleId, description, user);
    }

    public async Task AddRoleToSet(int setId, ulong roleId, string description, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setId, user);

        SocketRole role = Guild().GetRole(roleId);
        
        if (role == null)
            Exception("Could not find role");
        
        await RoleService.AddToSet(set!, role!, description);
        await React(Emoji.Parse(":+1:"));
    }

    
    // Remove
    public async Task RemoveRoleFromSet(string setName, string roleName, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setName, user);
        await RemoveRoleFromSet(set.Id, roleName, user);
    }

    public async Task RemoveRoleFromSet(int setId, string roleName, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setId, user);

        RoleEntry? entry = set.Roles.Find(x => x.Name == roleName);
        
        if (entry == null)
            Exception("Could not find role");

        await RoleService.RemoveFromSet(set, entry!.Id);
        await React(Emoji.Parse(":+1:"));
    }

    public async Task RemoveRoleFromSet(string setName, ulong roleId, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setName, user);
        await RoleService.RemoveFromSet(set, roleId);
        await React(Emoji.Parse(":+1:"));
    }

    public async Task RemoveRoleFromSet(int setId, ulong roleId, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setId, user);
        await RoleService.RemoveFromSet(set, roleId);
        await React(Emoji.Parse(":+1:"));
    }
    
    public async Task RemoveSet(string setName, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setName, user);

        if (user == null)
            user = User();

        await RoleService.DeleteSet(Guild().Id, user.Id, set);
        await React(Emoji.Parse(":+1:"));
    }
    
    public async Task RemoveSet(int setId, IUser? user = null)
    {
        RoleSet set = GetCurrentSet(setId, user);

        if (user == null)
            user = User();

        await RoleService.DeleteSet(Guild().Id, user.Id, set);
        await React(Emoji.Parse(":+1:"));
    }
}