using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Modules.Base;
using JevilNet.Services.Roles;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Modules.SlashCommands;

[RequireContext(ContextType.Guild)]
[Group("role", "role command group")]
public class Roles : InteractionModuleBase<SocketInteractionContext>, IRoleInterface
{
    public RoleService RoleService { get; set; }
    public IRoleInterface me => this;
    
    [SlashCommand("list", "Lists available role sets")]
    public Task ViewSetsCommand(IUser? user = null) => (user == null) ? me.GetRoleSets() : me.GetRoleSets(user.Id);

    [SlashCommand("show", "Creates an interaction for the specified set")]
    public Task CreateInteractionCommand([Autocomplete(typeof(RoleGuildAutocompleteHandler))] int setId)
        => me.ViewSet(setId);

    [SlashCommand("addset", "Create a role set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public Task CreateSetCommand(string setName) => me.AddRoleSet(setName);

    [SlashCommand("addnew", "Create and add a role to a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task CreateRoleCommand([Autocomplete(typeof(RoleGuildUserAutocompleteHandler))] int setId, string roleName,
        string description, IUser? user = null)
    {
        CheckPerms(user);
        await me.AddRoleToSet(setId, roleName, description, user);
    }

    [SlashCommand("add", "Add an existing role to a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task AddRoleCommand([Autocomplete(typeof(RoleGuildUserAutocompleteHandler))] int setId, IRole role,
        string description, IUser? user = null)
    {
        CheckPerms(user);
        await me.AddRoleToSet(setId, role.Id, description, user);
    }

    [SlashCommand("removeset", "Delete a role set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveRoleSetCommand([Autocomplete(typeof(RoleGuildUserAutocompleteHandler))] int setId,
        IUser? user = null)
    {
        CheckPerms(user);
        await me.RemoveSet(setId, user);
    }
    
    [SlashCommand("remove", "Remove a role from a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveRoleCommand([Autocomplete(typeof(RoleGuildUserAutocompleteHandler))] int setId, IRole role, IUser? user = null)
    {
        CheckPerms(user);
        await me.RemoveRoleFromSet(setId, role.Id, user);
    }

    public class RoleGuildUserAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            if (context.Guild == null)
                return AutocompletionResult.FromError(new Exception("Guild is null"));

            var roles = services.GetRequiredService<RoleService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();

            string user = (string)autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == "user")?.Value ?? context.User.Id.ToString();
            ulong userId = UInt64.Parse(user);
            
            var storage = roles.GetOrDefaultUserStorage(context.Guild.Id, userId).CustomStorage;
            return AutocompletionResult.FromSuccess(
                storage.Where(x => x.SetName.ToLower().Contains(search))
                    .Take(25)
                    .Select(x => new AutocompleteResult(x.SetName, x.Id)));
        }
    }
    
    public class RoleGuildAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            if (context.Guild == null)
                return AutocompletionResult.FromError(new Exception("Guild is null"));

            var roles = services.GetRequiredService<RoleService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();

            string user = (string)autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == "user")?.Value ?? context.User.Id.ToString();
            ulong userId = UInt64.Parse(user);

            var storage = roles.GetOrDefaultServerStorage(context.Guild.Id).GetCombinedStorage();
            return AutocompletionResult.FromSuccess(
                storage.Where(x => x.SetName.ToLower().Contains(search))
                    .Take(25)
                    .Select(x => new AutocompleteResult(x.SetName, x.Id)));
        }
    }

    private void CheckPerms(IUser? user = null, GuildPermission permission = GuildPermission.Administrator)
    {
        if (user != null)
        {
            var guildUser = Context.User as IGuildUser;
            if (!guildUser.GuildPermissions.Has(permission))
            {
                me.Exception("You are not allowed to do this");
            }
        }
    }
    
    public async Task Respond(string text = null, Embed embed = null, bool ephemeral = false, MessageComponent components = null)
        => await RespondAsync(text, embed: embed, ephemeral: ephemeral, components: components);
    public async Task React(IEmote emote) => await me.RespondEphermeral(emote.ToString());
    public SocketGuild Guild() => Context.Guild;
    public SocketUser User() => Context.User;
}