using System.ComponentModel;
using Discord;
using Discord.Commands;
using JevilNet.Attributes;
using JevilNet.Services;

namespace JevilNet.Modules.TextCommands;

public class Utils : ModuleBase<SocketCommandContext>
{
    public CommandHandler Handler { get; set; }
    
    [Command("ping")]
    [Alias("pong", "hello")]
    [Summary("Returns pong")]
    public Task PingAsync()
        => ReplyAsync("pong!");

    [Command("stop")]
    [Alias("kill")]
    [Summary("Stops the bot")]
    public async Task StopBot()
    {
        await ReplyAsync("Cya!");
        await Task.Delay(1000);
        await Handler.DeInitialiseAsync();
        await Context.Client.StopAsync();
        new Thread(() => // How am i supposed to shutdown normally?
        {
            Thread.Sleep(5000);
            Environment.Exit(0);
        }).Start();
        await Context.Client.LogoutAsync();
    }
    
    [Command("source")]
    [Summary("Gives a link to the source code of this bot")]
    public Task Source() => ReplyAsync("https://github.com/suchmememanyskill/JevilNet");
}