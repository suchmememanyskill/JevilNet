using Discord;
using Discord.Interactions;
using JevilNet.Extentions;
using JevilNet.Services.Quote;
using JevilNet.Services.Quote.Models;

namespace JevilNet.Modules.SlashCommands;

[Group("quote", "Access the quote module")]
[RequireContext(ContextType.Guild)]
public class Quote : InteractionModuleBase<SocketInteractionContext>
{
    public QuoteService QuoteService { get; set; }
    
    [SlashCommand("get", "Get a random or specific quote")]
    public async Task RandomQuote(int idx = -1)
    {
        List<string> combined = QuoteService.GetServerQuotes(Context.Guild.Id).GetCombinedQuotes();
        if (combined.Count <= 0)
        {
            await RespondAsync("No quotes have been added to this server", ephemeral: true);
            return;
        }

        string quote;
        
        if (idx <= 0)
            quote = combined[Program.Random.Next(combined.Count)];
        else
        {
            idx--;
            if (idx >= combined.Count)
                quote = "Index out of range";
            else
                quote = combined[idx];
        }

        await RespondAsync(quote, ephemeral: true);
    }
    
    [SlashCommand("add", "Adds a quote")]
    [RequireRole("Quoter")]
    public async Task AddQuote(string quote)
    {
        if (quote.Length > 300)
        {
            await RespondAsync("Quotes are limited to a max of 300 characters", ephemeral: true);
            return;
        }
        
        await QuoteService.AddQuote(Context.Guild.Id, Context.User.Id, Context.User.Username, quote);
        await RespondAsync("Quote added", ephemeral: true);
    }
    
    [SlashCommand("list", "Lists quotes from yourself or a specific user")]
    public async Task ListUser(IUser user = null, int page = 1)
    {
        if (user == null)
            user = Context.User;
        
        if (page < 1)
        {
            await RespondAsync("Invalid page", ephemeral: true);
            return;
        }

        ServerQuotes q = QuoteService.GetServerQuotes(Context.Guild.Id);
        UserQuotes userQuotes = QuoteService.GetUserQuotes(q, user.Id);
        page--;

        if (userQuotes.Quotes.Count <= 0)
        {
            await RespondAsync("You do not have any quotes", ephemeral:true);
            return;
        }

        string header = $"{user.Username}'s quotes: ({page + 1}/{(userQuotes.Quotes.Count + 19) / 20})\n\n";
        header += String.Join("\n", userQuotes
            .Quotes
            .Skip(page * 20)
            .Take(20)
            .Select((x, i) => $"{i + 1 + page * 20}: {x}"));
        
        await RespondAsync(header.SplitInParts(1900).First(), ephemeral:true);
    }
    
    [SlashCommand("del", "Deletes a specific quote from either you or someone else. Editing others requires admin")]
    public async Task DelUser(int idx, IUser user = null)
    {
        if (user == null)
            user = Context.User;
        else
        {
            var guildUser = Context.User as IGuildUser;
            if (!guildUser.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await RespondAsync("You cannot do this", ephemeral: true);
                return;
            }
        }
        
        try
        {
            await QuoteService.DelQuote(Context.Guild.Id, user.Id, idx - 1);
            await RespondAsync("Quote deleted", ephemeral: true);
        }
        catch (Exception e)
        {
            await RespondAsync(e.Message, ephemeral: true);
        }
    }
    
    [SlashCommand("edit", "Edits a specific quote from either you or someone else. Editing other requires admin")]
    public async Task EditUser(int idx, string newQuote, IUser user = null)
    {
        if (user == null)
            user = Context.User;
        else
        {
            var guildUser = Context.User as IGuildUser;
            if (!guildUser.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await RespondAsync("You cannot do this", ephemeral: true);
                return;
            }
        }
        
        try
        {
            await QuoteService.EditQuote(Context.Guild.Id, user.Id, idx - 1, newQuote);
            await RespondAsync("Quote edited", ephemeral: true);
        }
        catch (Exception e)
        {
            await RespondAsync(e.Message, ephemeral: true);
        }
    }
}