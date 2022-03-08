using Discord;
using JevilNet.Services;
using JevilNet.Services.CustomCommand;

namespace JevilNet.Modules.Base;

public interface ICustomCommand : IBaseInterface
{
    public CustomCommandService CommandService { get; set; }
    public MenuService MenuService { get; set; }

    public async Task GetCommands()
    {
        List<string> combined = CommandService.GetOrDefaultServerStorage(Guild().Id).GetCombinedStorage()
            .Select((x, i) => $"{i + 1}: {x.Caller}").ToList();

        MenuStorage menu = new(20, combined, $"{Guild().Name}'s Custom Commands");
        await MenuService.CreateMenu(
            async (embed, component) => await RespondEphermeral(embed: embed, components: component), menu);
    }
    
    public async Task GetCommandsFromUser(IUser? user = null)
    {
        user ??= User();
        
        List<string> combined = CommandService.GetOrDefaultUserStorage(Guild().Id, user.Id).CustomStorage
            .Select((x, i) => $"{i + 1}: {x.Caller}").ToList();

        MenuStorage menu = new(20, combined, $"{user.Username}'s Custom Commands");
        await MenuService.CreateMenu(
            async (embed, component) => await RespondEphermeral(embed: embed, components: component), menu);
    }

    public async Task AddCustomCommand(string caller, string content)
    {
        await CommandService.AddCommand(Guild().Id, User().Id, User().Username, caller, content);
        await React(Emoji.Parse(":+1:"));
    }

    public async Task RemoveCustomCommand(string caller, IUser? user = null)
    {
        user ??= User();
        await CommandService.RemoveCommand(Guild().Id, user.Id, caller);
        await React(Emoji.Parse(":+1:"));
    }
}