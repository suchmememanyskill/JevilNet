using System.Net;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace JevilNet.Modules.SlashCommands;

[RequireContext(ContextType.Guild)]
public class DiscordActivities : InteractionModuleBase<SocketInteractionContext>
{
    public record DiscordActivity(string displayName, string appId, PremiumTier tier = PremiumTier.None);

    public IConfiguration Config { get; set; }

    public static readonly List<DiscordActivity> activities = new()
    { // Taken from https://gist.github.com/GeneralSadaf/42d91a2b6a93a7db7a39208f2d8b53ad
        new("YouTube Watch Together",  "880218394199220334"),
        new("Poker Night", "755827207812677713", PremiumTier.Tier1),
        new("Betrayal.io", "773336526917861400"),
        new("Fishington.io", "814288819477020702"),
        new("Chess in the Park", "832012774040141894", PremiumTier.Tier1),
        new ("Sketch Heads",  "902271654783242291"),
        new ("Letter League",  "879863686565621790", PremiumTier.Tier1),
        new ("Word Snacks",  "879863976006127627"),
        new ("SpellCast",  "852509694341283871", PremiumTier.Tier1),
        new ("Checkers in the park",  "832013003968348200", PremiumTier.Tier1),
        new ("Blazing 8s",  "832025144389533716", PremiumTier.Tier1),
        new("Putt Party", "945737671223947305", PremiumTier.Tier1),
        new("Land-io", "903769130790969345", PremiumTier.Tier1),
        new("Bobble League", "947957217959759964", PremiumTier.Tier1),
        new("Ask Away", "976052223358406656"),
        new("Know What I Meme", "950505761862189096"),
        new("Bash Out", "1006584476094177371", PremiumTier.Tier1),
    };

    [SlashCommand("voiceactivity", "Creates an invite for a discord voice activity")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.CreateInstantInvite)]
    public async Task VoiceActivity([Autocomplete(typeof(DiscordActivityAutocomplete))] string appId, IVoiceChannel voice)
    {
        DiscordActivity? activity = activities.Find(x => x.appId == appId);

        if (activity == null)
            return;

        using (var req = new WebClient())
        {
            req.Headers["Authorization"] = $"Bot {Config["token"]}";
            req.Headers["Content-Type"] = "application/json";

            string url = $"https://discord.com/api/v9/channels/{voice.Id}/invites";
            string data =
                $"{{ \"max_age\": 600, \"max_uses\": 0, \"target_application_id\": \"{activity.appId}\", \"target_type\": 2, \"temporary\": false, \"validate\": null }}";
            
            string response = await req.UploadStringTaskAsync(new Uri(url), data);

            ApiResponseChannelInvite parsedResposne = JsonConvert.DeserializeObject<ApiResponseChannelInvite>(response)!;

            EmbedBuilder builder = new EmbedBuilder()
                .WithAuthor(Context.User)
                .WithThumbnailUrl(
                    $"https://cdn.discordapp.com/app-icons/{activity.appId}/{parsedResposne.App.Icon}.webp")
                .WithFooter("Expires at")
                .WithTimestamp(parsedResposne.ExpiresAt)
                .WithColor(Color.Orange)
                .WithTitle(parsedResposne.App.Name)
                .WithDescription(
                    $"[Click here to join the activity in {voice.Name}!](https://discord.com/invite/{parsedResposne.Code})");

            await RespondAsync(embed: builder.Build());
        }
    }
    
    public class DiscordActivityAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            if (context.Guild == null)
                return AutocompletionResult.FromError(new Exception("Guild is null"));
            
            bool showAll = false;
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();

            if (search.StartsWith("!"))
            {
                search = search[1..];
                showAll = true;
            }

            return AutocompletionResult.FromSuccess(activities
                .Where(x => showAll || x.tier <= context.Guild.PremiumTier)
                .Where(x => x.displayName.ToLower().Contains(search))
                .Select(x => new AutocompleteResult(x.displayName, x.appId))
                .ToList());
        }
    }
}