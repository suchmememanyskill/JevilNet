using Discord;
using Discord.Commands;
using JevilNet.Modules.Base;
using JevilNet.Services;

namespace JevilNet.Modules.TextCommands;

[RequireContext(ContextType.Guild)]
[Group("edit")]
public class ArbitraryEdit : TextCommandBase
{
    public ArbitraryEditService Service { get; set; }
    private IBaseInterface me => this;
    
    [Command]
    [Summary("Edits the last message sent in the channel")]
    public async Task ForceEdit([Remainder] string newMessage)
    {
        await Context.Message.DeleteAsync();
        await Service.Edit(Context.Channel as ITextChannel, newMessage);
    }

    [Command("webhook")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Summary("Sets the webhook for the current channel")]
    public async Task SetWebhook(string webhookUrl)
    {
        await Service.SetWebhook(Context.Guild.Id, Context.Channel.Id, webhookUrl);
        await Context.Message.DeleteAsync();
        await ReplyAsync("Webhook set!");
    }
    
    [Command("disable")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Priority(1)]
    [Summary("Removes the webhook for the current channel")]
    public async Task RemoveWebhook()
    {
        await Service.SetWebhook(Context.Guild.Id, Context.Channel.Id);
        await ReplyAsync("Webhook unset!");
    }
}