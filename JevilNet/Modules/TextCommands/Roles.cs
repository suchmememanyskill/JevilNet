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
public class Roles : ModuleBase<SocketCommandContext>, IRoleInterface
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
    public Task CreateRoleCommand(string setName, string roleName, [Remainder] string description)
        => me.AddRoleToSet(setName, roleName, description);
    
    [Command("add")]
    [Alias("create")]
    [Summary("Create and add a role to a set of another user")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task CreateRoleCommand(string setName, string roleName, IUser user, [Remainder] string description)
        => me.AddRoleToSet(setName, roleName, description, user);
    
    [Command("add")]
    [Alias("create")]
    [Priority(2)]
    [Summary("Add an existing role to a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task CreateRoleCommand(string setName, ulong roleId, [Remainder] string description)
        => me.AddRoleToSet(setName, roleId, description);
    
    [Command("add")]
    [Alias("create")]
    [Summary("Add an existing role to a set of another user")]
    [Priority(3)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task CreateRoleCommand(string setName, ulong roleId, IUser user, [Remainder] string description)
        => me.AddRoleToSet(setName, roleId, description, user);

    [Command("removeset")]
    [Alias("delset", "setremove", "setdel")]
    [Summary("Delete a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task RemoveSetCommand(string setName)
        => me.RemoveSet(setName);
    
    [Command("removeset")]
    [Alias("delset", "setremove", "setdel")]
    [Summary("Delete a set from another user")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task RemoveSetCommand(string setName, IUser user)
        => me.RemoveSet(setName, user);

    [Command("remove")]
    [Summary("Remove a role from a set")]
    [Priority(0)]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task RemoveRoleCommand(string setName, [Remainder] string roleName)
        => me.RemoveRoleFromSet(setName, roleName);
    
    [Command("remove")]
    [Summary("Remove a role from a set of another user")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task RemoveRoleCommand(string setName, IUser user, [Remainder] string roleName)
        => me.RemoveRoleFromSet(setName, roleName, user);
    
    [Command("remove")]
    [Summary("Remove a role from a set")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task RemoveRoleCommand(string setName, ulong roleId)
        => me.RemoveRoleFromSet(setName, roleId);
    
    [Command("remove")]
    [Priority(3)]
    [Summary("Remove a role from a set of another user")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task RemoveRoleCommand(string setName, IUser user, ulong roleId)
        => me.RemoveRoleFromSet(setName, roleId, user);
    

    public Task Respond(string text = null, Embed embed = null, bool ephemeral = false, MessageComponent components = null)
        => ReplyAsync(text, embed: embed, allowedMentions: AllowedMentions.None, components: components);
    public Task React(IEmote emote) => Context.Message.AddReactionAsync(emote);

    public SocketGuild Guild() => Context.Guild;

    public SocketUser User() => Context.User;
}