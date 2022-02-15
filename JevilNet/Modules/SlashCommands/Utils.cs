using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace JevilNet.Modules.SlashCommands;

public class Utils : InteractionModuleBase<SocketInteractionContext>
{
    public DiscordSocketClient Client { get; set; }

    [SlashCommand("say", "Sends a message on the bots behalf")]
    public async Task Say(string message, ISocketMessageChannel channel = null)
    {
        if (channel == null)
            await ReplyAsync(message, allowedMentions: AllowedMentions.None);
        else
            await channel.SendMessageAsync(message, allowedMentions: AllowedMentions.None);

        await RespondAsync("Message sent", ephemeral: true);
    }

    [SlashCommand("dm", "Dms a user a message")]
    public async Task Dm(IUser user, string message)
    {
        var dmChannel = await user.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync(message);
        await RespondAsync("Message sent", ephemeral: true);
    }

    [SlashCommand("game", "Sets the playing text on the bot")]
    public async Task SetGame(string game = "")
    {
        await Client.SetGameAsync(game);
        await RespondAsync((game == "") ? "Game unset" : "Game set", ephemeral: true);
    }
}