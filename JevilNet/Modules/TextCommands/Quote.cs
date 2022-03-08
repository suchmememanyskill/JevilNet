using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JevilNet.Attributes;
using JevilNet.Extentions;
using JevilNet.Modules.Base;
using JevilNet.Services;
using JevilNet.Services.Quote;
using JevilNet.Services.Roles;
using ContextType = Discord.Commands.ContextType;

namespace JevilNet.Modules.TextCommands;

[Group("quote")]
[Alias("quotes", "forcequote")]
[Summary("A module that posts a quote or an old message in the desired channel. People can only add quotes with the 'Quoter' role")]
[RequireContext(ContextType.Guild)]
public class Quote : TextCommandBase, IQuoteInterface
{
    public QuoteService QuoteService { get; set; }
    public MenuService MenuService { get; set; }
    private IQuoteInterface me => this;

    [Command]
    [Summary("Gets a random saved quote")]
    public async Task RandomQuote(int idx = -1) => await me.RandomQuoteInterface(idx);

    [Command("add")]
    [Summary("Adds a quote")]
    [RequireRole("Quoter")]
    public async Task AddQuote([Remainder] string quote) => await me.AddQuoteInterface(quote);

    [Command("list")]
    [Summary("Lists quotes from a specific user")]
    public async Task ListUser(IUser user, int page = 1) => await me.ListUserInterface(user, page);

    [Command("list")]
    [Summary("Lists quotes that you have added")]
    public Task ListSelf(int page = 1) => ListUser(Context.User, page);

    [Command("listall")]
    [Summary("Lists all quotes from this server")]
    public Task ListAll() => me.ListAllInterface();

    [Command("del")]
    [Alias("delete")]
    [Summary("Deletes a specific quote from a specific user")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DelUser(IUser user, int idx) => await me.DelUserInterface(idx, user);

    [Command("del")]
    [Alias("delete")]
    [Summary("Deletes a specific quote from yourself")]
    public Task DelSelf(int idx) => DelUser(Context.User, idx);

    [Command("edit")]
    [Summary("Edits another user's quote")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task EditUser(IUser user, int idx, [Remainder] string newQuote) =>
        await me.EditUserInterface(idx, newQuote, user);

    [Command("edit")]
    [Summary("Edit one of your quotes")]
    public Task EditSelf(int idx, [Remainder] string newQuote) => EditUser(Context.User, idx, newQuote);

    [Command("channel")]
    [Summary("Sets the quote channel for this server")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetQuoteChannel(ITextChannel target)
    {
        await QuoteService.SetQuoteChannel(Context.Guild.Id, target.Id);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }
    
    [Command("unsetchannel")]
    [Summary("Unsets the quote channel for this server")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task UnsetQuoteChannel()
    {
        await QuoteService.SetQuoteChannel(Context.Guild.Id, 0);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    public async Task RespondMultiple(IEnumerable<string> messages)
    {
        foreach (var message in messages)
            await Respond(message);
    }
}