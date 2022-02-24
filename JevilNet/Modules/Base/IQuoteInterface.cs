﻿using Discord;
using JevilNet.Extentions;
using JevilNet.Services.Quote;

namespace JevilNet.Modules.Base;

public interface IQuoteInterface : IBaseInterface
{
    public QuoteService QuoteService { get; set; }

    public Task RespondMultiple(IEnumerable<string> messages);
    
    public async Task RandomQuoteInterface(int idx)
    {
        List<string> combined = QuoteService.GetOrDefaultServerStorage(Guild().Id).GetCombinedStorage();
        if (combined.Count <= 0)
        {
            await RespondEphermeral("No quotes have been added to this server");
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

        await RespondEphermeral(quote);
    }
    
    public async Task AddQuoteInterface(string quote)
    {
        if (quote.Length > 300)
        {
            await RespondEphermeral("Quotes are limited to a max of 300 characters");
            return;
        }
        
        await QuoteService.AddQuote(Guild().Id, User().Id, User().Username, quote);
        await React(Emoji.Parse(":+1:"));
    }

    public async Task ListUserInterface(IUser? user = null, int page = 1)
    {
        if (user == null)
            user = User();
        
        if (page < 1)
        {
            await RespondEphermeral("Invalid page");
            return;
        }
        
        var userQuotes = QuoteService.GetOrDefaultUserStorage(Guild().Id, user.Id);
        page--;

        if (userQuotes.CustomStorage.Count <= 0)
        {
            await RespondEphermeral("You do not have any quotes");
            return;
        }

        string header = $"{user.Username}'s quotes: ({page + 1}/{(userQuotes.CustomStorage.Count + 19) / 20})\n\n";
        header += String.Join("\n", userQuotes
            .CustomStorage
            .Skip(page * 20)
            .Take(20)
            .Select((x, i) => $"{i + 1 + page * 20}: {x}"));

        await RespondMultiple(header.SplitInParts(1900));
    }
    
    public async Task DelUserInterface(int idx, IUser user = null)
    {
        if (user == null)
            user = User();

        try
        {
            await QuoteService.DelQuote(Guild().Id, user.Id, idx - 1);
            await React(Emoji.Parse(":+1:"));
        }
        catch (Exception e)
        {
            await RespondEphermeral(e.Message);
        }
    }
    
    public async Task EditUserInterface(int idx, string quote, IUser user = null)
    {
        if (user == null)
            user = User();

        try
        {
            await QuoteService.EditQuote(Guild().Id, user.Id, idx - 1, quote);
            await React(Emoji.Parse(":+1:"));
        }
        catch (Exception e)
        {
            await RespondEphermeral(e.Message);
        }
    }
}