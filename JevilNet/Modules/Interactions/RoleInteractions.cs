using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Modules.Base;
using JevilNet.Services.Roles;

namespace JevilNet.Modules.Interactions;

[RequireContext(ContextType.Guild)]
public class RoleInteractions : SlashCommandBase, IRoleInterface
{
    public RoleService RoleService { get; set; }
    private IRoleInterface me => this;

    [ComponentInteraction("roleselect:*")]
    public async Task RoleSelect(string strId, params string[] selections)
    {
        int id = int.Parse(strId);

        RoleSet? set = RoleService.GetOrDefaultServerStorage(Context.Guild.Id).GetCombinedStorage()
            .Find(x => x.Id == id);

        if (set == null)
            throw new Exception("Role set does not exist anymore?");

        List<ulong> selectedRoles = selections.Select(ulong.Parse).ToList();
        List<ulong> unselectedRoles = set.Roles.Where(x => !selectedRoles.Contains(x.Id)).Select(x => x.Id).ToList();
        
        SocketGuildUser user = Context.User as SocketGuildUser;

        foreach (var unselectedRole in unselectedRoles)
        {
            var role = Context.Guild.GetRole(unselectedRole);
            if (user.Roles.Any(x => x.Id == role.Id))
                await user.RemoveRoleAsync(role);
        }
        
        foreach (var selectedRole in selectedRoles)
        {
            var role = Context.Guild.GetRole(selectedRole);
            if (!user.Roles.Any(x => x.Id == role.Id))
                await user.AddRoleAsync(role);
        }

        await RespondAsync("Added/Removed roles!", ephemeral: true);
    }

    [ComponentInteraction("roleview:*")]
    public async Task RoleView(string strId)
    {
        int id = int.Parse(strId);
        await me.ViewSet(id);
    }
}