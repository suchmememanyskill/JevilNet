using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Modules.SlashCommands;

public class Utils : InteractionModuleBase<SocketInteractionContext>
{
    public DiscordSocketClient Client { get; set; }
    public EmoteService Emote { get; set; }
    public ArbitraryEditService Edit { get; set; }

    [SlashCommand("say", "Sends a message on the bots behalf")]
    public async Task Say(string message, ISocketMessageChannel channel = null)
    {
        if (channel == null)
            await ReplyAsync(message, allowedMentions: AllowedMentions.None);
        else
            await channel.SendMessageAsync(message, allowedMentions: AllowedMentions.None);

        await RespondAsync("Message sent", ephemeral: true);
    }

    [SlashCommand("dm", "Dms a user a message")]
    public async Task Dm(IUser user, string message)
    {
        var dmChannel = await user.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync(message);
        await RespondAsync("Message sent", ephemeral: true);
    }

    [SlashCommand("game", "Sets the playing text on the bot")]
    public async Task SetGame(string game = "")
    {
        await Client.SetGameAsync(game);
        await RespondAsync((game == "") ? "Game unset" : "Game set", ephemeral: true);
    }

    [SlashCommand("edit", "Imposes as or edits the last message in a channel")]
    public async Task EditMessage(string newMessage, bool edit = false)
    {
        await DeferAsync(true);
        try
        {
            await Edit.Edit(Context.Channel as ITextChannel, newMessage);
            await FollowupAsync("Sent message");
        }
        catch (Exception e)
        {
            await FollowupAsync(e.Message);
        }
    }
    
    [SlashCommand("emote", "Sends an emoji on the bots behalf")]
    public async Task SendEmote([Autocomplete(typeof(EmoteAutocompleteHandler))] string emote)
    {
        GuildEmote? guildEmote = Emote.FindEmote(emote);
        if (guildEmote != null)
            await RespondAsync(guildEmote.ToString());
        else
            await RespondAsync("Didn't find the specified emote", ephemeral:true);
    }

    public class EmoteAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            EmoteService emoteService = services.GetRequiredService<EmoteService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();
            
            return AutocompletionResult.FromSuccess(emoteService.CachedEmotesList.Where(x => x.Name.ToLower().Contains(search)).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name)));
        }
    }
}