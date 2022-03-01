using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Extentions;
using JevilNet.Modules.Base;
using JevilNet.Services.Quote;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Modules.SlashCommands;

[Group("quote", "Access the quote module")]
[RequireContext(ContextType.Guild)]
public class Quote : InteractionModuleBase<SocketInteractionContext>, IQuoteInterface
{
    public QuoteService QuoteService { get; set; }
    private IQuoteInterface me => this;

    [SlashCommand("get", "Get a random or specific quote")]
    public async Task RandomQuote(int idx = -1) => await me.RandomQuoteInterface(idx);

    [SlashCommand("add", "Adds a quote")]
    [RequireRole("Quoter")]
    public async Task AddQuote(string quote) => await me.AddQuoteInterface(quote);

    [SlashCommand("list", "Lists quotes from yourself or a specific user")]
    public async Task ListUser(IUser user = null, int page = 1) => await me.ListUserInterface(user, page);

    [SlashCommand("del", "Deletes a specific quote from either you or someone else. Editing others requires admin")]
    public async Task DelUser(int idx, IUser user = null)
    {
        if (user != null)
        {
            var guildUser = Context.User as IGuildUser;
            if (!guildUser.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await RespondAsync("You cannot do this", ephemeral: true);
                return;
            }
        }

        await me.DelUserInterface(idx, user);
    }
    
    [SlashCommand("edit", "Edits a specific quote from either you or someone else. Editing other requires admin")]
    public async Task EditUser([Autocomplete(typeof(QuoteSearchAutocompleteHandler))] int idx, string newQuote, IUser user = null)
    {
        if (user != null)
        {
            var guildUser = Context.User as IGuildUser;
            if (!guildUser.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await RespondAsync("You cannot do this", ephemeral: true);
                return;
            }
        }

        await me.EditUserInterface(idx, newQuote, user);
    }

    public class QuoteSearchAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            var quote = services.GetRequiredService<QuoteService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();

            //return AutocompletionResult.FromSuccess(quote.GetOrDefaultUserStorage(context.Guild.Id, context.User.Id))
            return AutocompletionResult.FromError(new Exception("fuk"));
        }
    }

    public async Task Respond(string text = null, Embed embed = null, bool ephemeral = false)
        => await RespondAsync(text, embed: embed, ephemeral: ephemeral);
    public async Task React(IEmote emote) => await me.RespondEphermeral(emote.ToString());
    public SocketGuild Guild() => Context.Guild;
    public SocketUser User() => Context.User;
    public Task RespondMultiple(IEnumerable<string> messages) => me.RespondEphermeral(messages.First());
}