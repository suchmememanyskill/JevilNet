using Discord;
using Discord.WebSocket;

namespace JevilNet.Modules.Base;

public interface IBaseInterface
{
    Task Respond(string text = null, Embed embed = null, bool ephemeral = false);
    Task RespondEphermeral(string text = null, Embed embed = null) => Respond(text, embed, true);
    Task React(IEmote emote);
    SocketGuild Guild();
    SocketUser User();
}