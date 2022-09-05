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

        string buff = "";
        foreach (var x in gifts.Select(x => $"{x.GameName} (Type: {x.Type}): `{x.GameKey}`"))
        {
            buff += x + "\n";
            if (buff.Length >= 1800)
            {
                await FollowupAsync(buff, ephemeral: true);
                buff = "";
            }
        }

        await FollowupAsync(buff, ephemeral: true);
    }

    private static int SPLIT_AMOUNT = 25;
    [SlashCommand("list", "Lists all available gifts")]
    public async Task GiftList()
    {
        if (GiftService.cachedGifts.Count <= 0)
        {
            await me.RespondEphermeral("No gifts are available");
            return;
        }

        SelectMenuBuilder? selectMenuBuilder = null;
        var componentBuilder = new ComponentBuilder();
        
        for (int i = 0; i < GiftService.cachedGifts.Count; i++)
        {
            GiftCarrier gift = GiftService.cachedGifts[i];

            if (i % SPLIT_AMOUNT == 0)
            {
                if (selectMenuBuilder != null)
                    componentBuilder.WithSelectMenu(selectMenuBuilder);
                
                selectMenuBuilder = new SelectMenuBuilder()
                    .WithCustomId($"giftmenu:{i}")
                    .WithMaxValues(1)
                    .WithMaxValues(1);
            }

            selectMenuBuilder!.AddOption(gift.GameName, gift.GameId.ToString(),
                $"{gift.Gifts.Count} gift(s) available (Platform: {gift.GiftType})");
        }
        
        componentBuilder.WithSelectMenu(selectMenuBuilder);
        await me.RespondEphermeral("Available gifts:", components: componentBuilder.Build());
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