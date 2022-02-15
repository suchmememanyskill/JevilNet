using System.ComponentModel;
using Discord;
using Discord.Commands;
using JevilNet.Attributes;
using JevilNet.Services;

namespace JevilNet.Modules.TextCommands;

public class Utils : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Alias("pong", "hello")]
    [Summary("Returns pong")]
    public Task PingAsync()
        => ReplyAsync("pong!");
}