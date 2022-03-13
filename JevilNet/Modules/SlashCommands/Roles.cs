using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Modules.Base;
using JevilNet.Services.Roles;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Modules.SlashCommands;

[RequireContext(ContextType.Guild)]
[Group("role", "role command group")]
public class Roles : SlashCommandBase, IRoleInterface
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
        string description)
    {
        await me.AddRoleToSet(setId, roleName, description);
    }

    [SlashCommand("add", "Add an existing role to a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task AddRoleCommand([Autocomplete(typeof(RoleGuildUserAutocompleteHandler))] int setId, IRole role,
        string description)
    {
        await me.AddRoleToSet(setId, role.Id, description);
    }

    [SlashCommand("removeset", "Delete a role set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveRoleSetCommand([Autocomplete(typeof(RoleGuildUserAutocompleteHandler))] int setId)
    {
        await me.RemoveSet(setId);
    }
    
    [SlashCommand("remove", "Remove a role from a set")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public async Task RemoveRoleCommand([Autocomplete(typeof(RoleGuildUserAutocompleteHandler))] int setId, IRole role)
    {
        await me.RemoveRoleFromSet(setId, role.Id);
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
            
            List<RoleSet> storage;

            if ((context.User as IGuildUser).GuildPermissions.Has(GuildPermission.Administrator))
                storage = roles.GetOrDefaultServerStorage(context.Guild.Id).GetCombinedStorage();
            else
                storage = roles.GetOrDefaultUserStorage(context.Guild.Id, context.User.Id).CustomStorage;
            
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

            List<RoleSet> storage = roles.GetOrDefaultServerStorage(context.Guild.Id).GetCombinedStorage();
            return AutocompletionResult.FromSuccess(
                storage.Where(x => x.SetName.ToLower().Contains(search))
                    .Take(25)
                    .Select(x => new AutocompleteResult(x.SetName, x.Id)));
        }
    }
}