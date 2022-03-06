using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace JevilNet.Modules.Base;

public abstract class SlashCommandBase : InteractionModuleBase<SocketInteractionContext>, IBaseInterface
{
    private IBaseInterface me => this;
    public async Task Respond(string text = null, Embed embed = null, bool ephemeral = false, MessageComponent components = null)
        => await RespondAsync(text, embed: embed, ephemeral: ephemeral, components: components);
    public async Task React(IEmote emote) => await me.RespondEphermeral(emote.ToString());
    public SocketGuild Guild() => Context.Guild;
    public SocketUser User() => Context.User;
}