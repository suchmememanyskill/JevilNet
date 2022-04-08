using Discord;
using Discord.Commands;
using JevilNet.Modules.Base;
using JevilNet.Services;
using JevilNet.Services.CustomCommand;

namespace JevilNet.Modules.TextCommands;

[Group("command")]
[Alias("tag")]
[Summary("A module to add custom commands to the bot")]
[RequireContext(ContextType.Guild)]
public class CustomCommands : TextCommandBase, ICustomCommand
{
    public CustomCommandService CommandService { get; set; }
    public MenuService MenuService { get; set; }
    private ICustomCommand me => this;

    [Command]
    [Summary("Lists available custom commands")]
    public Task Get(IUser? user = null) => (user == null) ? me.GetCommands() : me.GetCommandsFromUser(user);

    [Command("list")]
    [Summary("List custom commands from user")]
    public Task List(IUser? user = null) => me.GetCommandsFromUser(user);

    [Command("add")]
    [Summary("Add a custom command")]
    public Task Add(string command, [Remainder] string message) => me.AddCustomCommand(command, message);

    [Command("remove")]
    [Alias("del")]
    [Summary("Remove a custom command")]
    public Task Remove(string command) => me.RemoveCustomCommand(command);
    
    [Command("remove")]
    [Alias("del")]
    [Summary("Remove a custom command from a specific user")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task Remove(string command, IUser user) => me.RemoveCustomCommand(command, user);
}