using Discord;
using Discord.WebSocket;
using JevilNet.Modules.SlashCommands;
using JevilNet.Services.Roles;
using JevilNet.Services.UserSpecificGuildStorage;

namespace JevilNet.Modules.Base;

public interface IRoleInterface : IBaseInterface
{
    public RoleService RoleService { get; set; }

    public Tuple<UserStorage<RoleSet>, RoleSet> GetCurrentSet(string setName)
    {
        var userStorage = RoleService.GetSetOwner(Guild().Id, setName);
        
        if (userStorage == null)
            Exception("Did not find set");
        
        if (userStorage!.UserId != User().Id)
            ThrowOnMissingPerms();
        
        var set = RoleService.GetRoleSet(Guild().Id, userStorage!.UserId, setName);

        return new(userStorage!, set!);
    }
    
    public Tuple<UserStorage<RoleSet>, RoleSet> GetCurrentSet(int setId)
    {
        var userStorage = RoleService.GetSetOwner(Guild().Id, setId);
        
        if (userStorage == null)
            Exception("Did not find set");
        
        if (userStorage!.UserId != User().Id)
            ThrowOnMissingPerms();
        
        var set = RoleService.GetRoleSet(Guild().Id, userStorage!.UserId, setId);

        return new(userStorage!, set!);
    }

    // Get
    public async Task GetRoleSets(ulong userId = 0)
    {
        string name;
        List<RoleSet> roleSets;

        if (userId == 0)
        {
            roleSets = RoleService.GetOrDefaultServerStorage(Guild().Id).GetCombinedStorage();
            name = Guild().Name;
        }
        else
        {
            UserStorage<RoleSet>? user = RoleService.GetOrDefaultServerStorage(Guild().Id).UserStorage
                .Find(x => x.UserId == userId);
            
            if (user == null)
                Exception("User not found");

            roleSets = user!.CustomStorage;
            name = $"{Guild().Name} from {user.UserName}";
        }
            
        
        if (roleSets.Count <= 0)
            Exception("Server/User has no role sets");

        ComponentBuilder builder = new ComponentBuilder();
        roleSets.ForEach(x => builder.WithButton(x.SetName, $"roleview:{x.Id.ToString()}"));
        await RespondEphermeral($"Role sets in {name}", components: builder.Build());
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
        
        set.Roles.ForEach(x => menu.AddOption(x.Name, x.Id.ToString(), (x.Description == "-") ? null : x.Description));

        await RespondEphermeral($"Pick roles from {set.SetName}!", components:  new ComponentBuilder().WithSelectMenu(menu).Build());
    }

    
    // Add
    public async Task AddRoleSet(string setName)
    {
        await RoleService.CreateSet(Guild().Id, User().Id, User().Username, setName);
        await React(Emoji.Parse(":+1:"));
    }
    

    public async Task AddRoleToSet(string setName, string roleName, string description)
    {
        var set = GetCurrentSet(setName);
        await AddRoleToSet(set.Item2.Id, roleName, description);
    }

    public async Task AddRoleToSet(int setId, string roleName, string description)
    {
        var set = GetCurrentSet(setId);
        
        await RoleService.AddToSet(Guild(), set.Item2, roleName, description);
        await React(Emoji.Parse(":+1:"));
    }
    
    public async Task AddRoleToSet(string setName, ulong roleId, string description)
    {
        var set = GetCurrentSet(setName);
        await AddRoleToSet(set.Item2.Id, roleId, description);
    }

    public async Task AddRoleToSet(int setId, ulong roleId, string description)
    {
        var set = GetCurrentSet(setId);

        SocketRole role = Guild().GetRole(roleId);
        
        if (role == null)
            Exception("Could not find role");

        if (User() is SocketGuildUser guildUser)
        {
            int maxRolePos = guildUser.Roles.Max(x => x.Position);
            if (role!.Position >= maxRolePos)
                Exception("Cannot add role that is equal or higher than your current role");
        }
        else
            Exception("Not in a guild?");

        await RoleService.AddToSet(set.Item2!, role!, description);
        await React(Emoji.Parse(":+1:"));
    }

    
    // Remove
    public async Task RemoveRoleFromSet(string setName, string roleName)
    {
        var set = GetCurrentSet(setName);
        await RemoveRoleFromSet(set.Item2.Id, roleName);
    }

    public async Task RemoveRoleFromSet(int setId, string roleName)
    {
        var set = GetCurrentSet(setId);

        RoleEntry? entry = set.Item2.Roles.Find(x => x.Name == roleName);
        
        if (entry == null)
            Exception("Could not find role");

        await RoleService.RemoveFromSet(set.Item2, entry!.Id);
        await React(Emoji.Parse(":+1:"));
    }

    public async Task RemoveRoleFromSet(string setName, ulong roleId)
    {
        var set = GetCurrentSet(setName);
        await RoleService.RemoveFromSet(set.Item2, roleId);
        await React(Emoji.Parse(":+1:"));
    }

    public async Task RemoveRoleFromSet(int setId, ulong roleId)
    {
        var set = GetCurrentSet(setId);
        await RoleService.RemoveFromSet(set.Item2, roleId);
        await React(Emoji.Parse(":+1:"));
    }
    
    public async Task RemoveSet(string setName)
    {
        var set = GetCurrentSet(setName);

        await RoleService.DeleteSet(Guild().Id, set.Item1.UserId, set.Item2);
        await React(Emoji.Parse(":+1:"));
    }
    
    public async Task RemoveSet(int setId)
    {
        var set = GetCurrentSet(setId);

        await RoleService.DeleteSet(Guild().Id, set.Item1.UserId, set.Item2);
        await React(Emoji.Parse(":+1:"));
    }
}