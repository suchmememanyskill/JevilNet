using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JevilNet.Modules.Base;
using JevilNet.Services.Roles;

namespace JevilNet.Modules.TextCommands;

[Group("role")]
[Alias("roles")]
[Summary("A module that allows users to apply their own roles")]
[RequireContext(ContextType.Guild)]
public class Roles : TextCommandBase, IRoleInterface
{
    public RoleService RoleService { get; set; }
    public IRoleInterface me => this;

    [Command]
    [Summary("Lists the available role sets")]
    public Task ViewSetsCommand() => me.GetRoleSets();
    
    [Command]
    [Summary("Creates an interaction for the specified set")]
    public Task ViewSetCommand(string setName) => me.ViewSet(setName);

    [Command("list")]
    [Summary("List available role sets")]
    public Task ViewSetsCommand(IUser? user = null) => (user == null) ? me.GetRoleSets() : me.GetRoleSets(user.Id);

    [Command("addset")]
    [Alias("setadd", "setcreate", "createset")]
    [Summary("Creates a role set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task CreateSetCommand(string setName) => me.AddRoleSet(setName);

    [Command("add")]
    [Alias("create")]
    [Priority(0)]
    [Summary("Create and add a role to a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task CreateRoleCommand(string setName, string roleName, [Remainder] string description)
        => await me.AddRoleToSet(setName, roleName, description);

    [Command("add")]
    [Alias("create")]
    [Priority(1)]
    [Summary("Add an existing role to a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task CreateRoleCommand(string setName, ulong roleId, [Remainder] string description)
        => me.AddRoleToSet(setName, roleId, description);

    [Command("removeset")]
    [Alias("delset", "setremove", "setdel")]
    [Summary("Delete a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task RemoveSetCommand(string setName)
        => me.RemoveSet(setName);

    [Command("remove")]
    [Summary("Remove a role from a set")]
    [Priority(0)]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task RemoveRoleCommand(string setName, [Remainder] string roleName)
        => me.RemoveRoleFromSet(setName, roleName);

    [Command("remove")]
    [Summary("Remove a role from a set")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task RemoveRoleCommand(string setName, ulong roleId)
        => me.RemoveRoleFromSet(setName, roleId);

}