using Discord;
using Discord.Commands;
using JevilNet.Attributes;
using JevilNet.Extentions;
using JevilNet.Services.Quote;
using JevilNet.Services.Quote.Models;
using ContextType = Discord.Commands.ContextType;

namespace JevilNet.Modules.TextCommands;

[Group("quote")]
[Alias("quotes", "forcequote")]
[Summary("A module that posts a quote or an old message in the desired channel. People can only add quotes with the 'Quoter' role")]
[RequireContext(ContextType.Guild)]
public class Quote : ModuleBase<SocketCommandContext>
{
    public QuoteService QuoteService { get; set; }

    [Command]
    [Summary("Gets a random saved quote")]
    public async Task RandomQuote()
    {
        List<string> combined = QuoteService.GetServerQuotes(Context.Guild.Id).GetCombinedQuotes();
        if (combined.Count <= 0)
        {
            await ReplyAsync("No quotes have been added to this server");
            return;
        }

        string random = combined[Program.Random.Next(combined.Count)];
        await ReplyAsync(random, allowedMentions: AllowedMentions.None);
    }

    [Command("add")]
    [Summary("Adds a quote")]
    [RequireRole("Quoter")]
    public async Task AddQuote([Remainder] string quote)
    {
        if (quote.Length > 300)
        {
            await ReplyAsync("Quotes are limited to a max of 300 characters");
            return;
        }
        
        await QuoteService.AddQuote(Context.Guild.Id, Context.User.Id, Context.User.Username, quote);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    [Command("listuser")]
    [Summary("Lists quotes from a specific user")]
    public async Task ListUser(IUser user, int page = 1)
    {
        if (page < 1)
        {
            await ReplyAsync("Invalid page");
            return;
        }

        ServerQuotes q = QuoteService.GetServerQuotes(Context.Guild.Id);
        UserQuotes userQuotes = QuoteService.GetUserQuotes(q, user.Id);
        page--;

        if (userQuotes.Quotes.Count <= 0)
        {
            await ReplyAsync("You do not have any quotes");
            return;
        }

        string header = $"{user.Username}'s quotes: ({page + 1}/{(userQuotes.Quotes.Count + 19) / 20})\n\n";
        header += String.Join("\n", userQuotes
            .Quotes
            .Skip(page * 20)
            .Take(20)
            .Select((x, i) => $"{i + 1 + page * 20}: {x}"));

        foreach (var part in header.SplitInParts(1900)) 
            await ReplyAsync(part, allowedMentions: AllowedMentions.None);
    }

    [Command("list")]
    [Summary("Lists quotes that you have added")]
    public Task ListSelf(int page = 1) => ListUser(Context.User, page);

    [Command("deluser")]
    [Alias("deleteuser")]
    [Summary("Deletes a specific quote from a specific user")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DelUser(IUser user, int idx)
    {
        try
        {
            await QuoteService.DelQuote(Context.Guild.Id, user.Id, idx - 1);
            await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
        }
        catch (Exception e)
        {
            await ReplyAsync(e.Message);
        }
    }

    [Command("del")]
    [Alias("delete")]
    [Summary("Deletes a specific quote from yourself")]
    public Task DelSelf(int idx) => DelUser(Context.User, idx);

    [Command("edituser")]
    [Summary("Edits another user's quote")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task EditUser(IUser user, int idx, [Remainder] string newQuote)
    {
        try
        {
            await QuoteService.EditQuote(Context.Guild.Id, user.Id, idx - 1, newQuote);
            await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
        }
        catch (Exception e)
        {
            await ReplyAsync(e.Message);
        }
    }

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
}