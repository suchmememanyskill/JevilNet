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
    
    public EmoteService Emote { get; set; }

    [Command("emoji")]
    [Alias("emote")]
    [Summary("Sends an emote of your choosing")]
    public async Task SendEmote(string emote)
    {
        GuildEmote? guildEmote = Emote.FindEmote(emote);
        if (guildEmote != null)
            await ReplyAsync(guildEmote.ToString());
        else
            await ReplyAsync("Didn't find the specified emote");
    }

    [Command("ping")]
    [Alias("pong", "hello")]
    [Summary("Returns pong")]
    public Task PingAsync()
        => ReplyAsync("pong!");

    [Command("stop")]
    [Alias("kill")]
    [Summary("Stops the bot")]
    [RequireOwner]
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
    [Summary("Sends a message on the bots behalf. Put in a channel id to send in that specific channel")]
    public async Task Say([Remainder] string message)
    {
        string[] split = message.Split(" ", 2);
        if (ulong.TryParse(split[0], out ulong result))
        {
            var channel = await Client.GetChannelAsync(result);
            if (channel is ITextChannel textChannel)
            {
                await textChannel.SendMessageAsync(split[1], allowedMentions: AllowedMentions.None);
                await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
                return;
            }
        }
        
        await ReplyAsync(message, allowedMentions: AllowedMentions.None);
        await Context.Message.DeleteAsync();
    }

    [Command("dmid")]
    [Summary("Dms a user a message via a user id")]
    public async Task DmId(ulong userId, [Remainder] string message)
    {
        var user = await Client.GetUserAsync(userId);
        var dmChannel = await user.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync(message);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    [Command("game")]
    [Summary("Sets the playing text on the bot")]
    public async Task SetGame([Remainder] string game = "")
    {
        await Client.SetGameAsync(game);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }
}