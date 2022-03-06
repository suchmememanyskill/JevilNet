using Discord;
using Discord.WebSocket;

namespace JevilNet.Modules.Base;

public interface IBaseInterface
{
    Task Respond(string text = null, Embed embed = null, bool ephemeral = false, MessageComponent components = null);
    Task RespondEphermeral(string text = null, Embed embed = null, MessageComponent components = null) => Respond(text, embed, true, components);
    Task React(IEmote emote);
    SocketGuild Guild();
    SocketUser User();
    void Exception(string message) => throw new Exception(message);
    
    void ThrowOnMissingPerms(GuildPermission permission = GuildPermission.Administrator)
    {
        var guildUser = User() as IGuildUser;
        if (!guildUser!.GuildPermissions.Has(permission))
            Exception("You are not allowed to do this");
    }
}