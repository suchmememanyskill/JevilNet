using Discord;
using Discord.Interactions;
using JevilNet.Extentions;
using JevilNet.Modules.Base;
using JevilNet.Services.Gift;

namespace JevilNet.Modules.SlashCommands;

[Group("gift", "Interact with the key gift system")]
public class GiftSlashCommands : SlashCommandBase
{
    public GiftService GiftService { get; set; }
    private IBaseInterface me => this;

    [SlashCommand("add", "Add a gift to the gift pool")]
    public async Task GiftAdd(GiftType type, [Autocomplete(typeof(GameAddAutocompleteHandler))] string gameName,
        string key)
    {
        if (type == GiftType.Steam)
        {
            if (long.TryParse(gameName, out long result))
            {
                await GiftService.AddSteamKey(me.Guild().Id, me.User().Id, me.User().Username, result, key);
            }
            else
            {
                await GiftService.AddSteamKey(me.Guild().Id, me.User().Id, me.User().Username, gameName, key);
                return;
            }
        }
        else
        {
            await GiftService.AddCustomKey(me.Guild().Id, me.User().Id, me.User().Username, gameName, key);
        }

        await RespondAsync("Added key", ephemeral: true);
    }

    [SlashCommand("mine", "Show keys owned by you")]
    public async Task GiftMine()
    {
        var gifts = GiftService.GetAllGiftsOfUser(me.User().Id);
        if (gifts.Count <= 0)
        {
            await me.RespondEphermeral("You have no gifts");
            return;
        }

        await DeferAsync(true);

        var channel = await me.User().CreateDMChannelAsync();

        string buff = "";
        foreach (var x in gifts.Select(x => $"{x.GameName} (Type: {x.Type}): `{x.GameKey}`"))
        {
            buff += x + "\n";
            if (buff.Length >= 1800)
            {
                await channel.SendMessageAsync(buff);
                buff = "";
            }
        }

        await channel.SendMessageAsync(buff);
        await FollowupAsync("Dm'd you all your saved keys", ephemeral: true);
        await FollowupAsync("Dm'd you all your saved keys", ephemeral: true);
    }
    
    public class GameAddAutocompleteHandler : AutocompleteHandler
    {
        public GiftService GiftService { get; set; }
        
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            GiftType type = GiftType.Custom;

            if ((string) (autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == "type")?.Value ?? "") ==
                "Steam")
                type = GiftType.Steam;

            if (type == GiftType.Custom)
            {
                return AutocompletionResult.FromError(new NotImplementedException());
            }

            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();

            if (long.TryParse(search, out long result))
            {
                SteamApp? app = GiftService.SteamApps.Find(x => x.AppId == result);

                if (app != null)
                {
                    return AutocompletionResult.FromSuccess(new List<AutocompleteResult>() { new (app.Name.Truncate(100), app.AppId.ToString()) });
                }
            }
            
            return AutocompletionResult.FromSuccess(GiftService.SteamApps.Where(x => x.Name.ToLower().Contains(search) && !string.IsNullOrWhiteSpace(x.Name)).Take(25).Select(x => new AutocompleteResult(x.Name.Truncate(100), x.AppId.ToString())));
        }
    }
}