using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace JevilNet.Modules.Base;

public class TextCommandBase : ModuleBase<SocketCommandContext>, IBaseInterface
{
    public Task Respond(string text = null, Embed embed = null, bool ephemeral = false, MessageComponent components = null)
        => ReplyAsync(text, embed: embed, allowedMentions: AllowedMentions.None, components: components);
    public Task React(IEmote emote) => Context.Message.AddReactionAsync(emote);

    public SocketGuild Guild() => Context.Guild;

    public SocketUser User() => Context.User;
}