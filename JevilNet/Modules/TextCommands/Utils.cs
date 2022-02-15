using System.ComponentModel;
using Discord;
using Discord.Commands;
using JevilNet.Attributes;
using JevilNet.Services;

namespace JevilNet.Modules.TextCommands;

public class Utils : ModuleBase<SocketCommandContext>
{
    [TextCommand("ping")]
    [Alias("pong", "hello")]
    [Summary("Returns pong")]
    public Task PingAsync()
        => ReplyAsync("pong!");
/*
    [TextCommand("help", "Shows this message")]
    public Task GenerateAndShowHelp() => ReplyAsync(TextHelp.GenerateHelp());
*/
}