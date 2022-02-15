using System.ComponentModel;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JevilNet.Attributes;
using JevilNet.Services;

namespace JevilNet.Modules.TextCommands;

public class Utils : ModuleBase<SocketCommandContext>
{
    public DiscordSocketClient Client { get; set; }
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

    [Command("say")]
    [Summary("Sends a message on the bots behalf")]
    public async Task Say(string message, ITextChannel channel = null)
    {
        if (channel == null)
        {
            await ReplyAsync(message, allowedMentions: AllowedMentions.None);
        }
        else
        {
            await channel.SendMessageAsync(message, allowedMentions: AllowedMentions.None);
            await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
        }
    }

    [Command("dm")]
    [Summary("Dms a user a message")]
    public async Task Dm(IUser user, string message)
    {
        var dmChannel = await user.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync(message);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    [Command("game")]
    [Summary("Sets the playing text on the bot")]
    public async Task SetGame(string game = "")
    {
        await Client.SetGameAsync(game);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }
}