﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Extentions;
using JevilNet.Modules.Base;
using JevilNet.Services;
using JevilNet.Services.Quote;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Modules.SlashCommands;

[Group("quote", "Access the quote module")]
[RequireContext(ContextType.Guild)]
public class Quote : SlashCommandBase, IQuoteInterface
{
    public QuoteService QuoteService { get; set; }
    public MenuService MenuService { get; set; }
    private IQuoteInterface me => this;

    [SlashCommand("get", "Get a random or specific quote")]
    public async Task RandomQuote(int idx = -1) => await me.RandomQuoteInterface(idx);

    [SlashCommand("add", "Adds a quote")]
    [RequireRole("Quoter")]
    public async Task AddQuote(string quote) => await me.AddQuoteInterface(quote);

    [SlashCommand("list", "Lists quotes from yourself or a specific user")]
    public async Task ListUser(IUser user = null, int page = 1) => await me.ListUserInterface(user, page);
    
    [SlashCommand("listall", "Lists all quotes from this server")]
    public Task ListAll() => me.ListAllInterface();

    [SlashCommand("del", "Deletes a specific quote from either you or someone else. Editing others requires admin")]
    public async Task DelUser([Autocomplete(typeof(QuoteSearchAutocompleteHandler))] int idx, IUser user = null)
    {
        if (user != null)
            me.ThrowOnMissingPerms();

        await me.DelUserInterface(idx, user);
    }
    
    [SlashCommand("edit", "Edits a specific quote from either you or someone else. Editing other requires admin")]
    public async Task EditUser([Autocomplete(typeof(QuoteSearchAutocompleteHandler))] int idx, string newQuote, IUser user = null)
    {
        if (user != null)
            me.ThrowOnMissingPerms();

        await me.EditUserInterface(idx, newQuote, user);
    }

    public class QuoteSearchAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            if (context.Guild == null)
                return AutocompletionResult.FromError(new Exception("Guild is null"));
            
            var quote = services.GetRequiredService<QuoteService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();

            string user = (string)autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == "user")?.Value ?? context.User.Id.ToString();
            ulong userId = UInt64.Parse(user);


            var storage = quote.GetOrDefaultUserStorage(context.Guild.Id, userId).CustomStorage;
            return AutocompletionResult.FromSuccess(
                    storage.Where(x => x.ToLower().Contains(search))
                    .Take(25)
                    .Select(x => new AutocompleteResult(x.Truncate(100), storage.FindIndex(y => y == x) + 1)));
        }
    }
    
    public Task RespondMultiple(IEnumerable<string> messages) => me.RespondEphermeral(messages.First());
}