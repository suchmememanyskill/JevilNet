using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Modules.Base;
using JevilNet.Services.Gift;

namespace JevilNet.Modules.Interactions;

public class GiftInteraction : SlashCommandBase
{
    public GiftService GiftService { get; set; }
    protected bool forceEphemeral = false;

    public async Task RespondEphemeral(string text = null, Embed embed = null, MessageComponent components = null)
    {
        if (!forceEphemeral && Context.Interaction is SocketMessageComponent)
        {
            SocketMessageComponent interaction = Context.Interaction as SocketMessageComponent;
            await interaction.UpdateAsync(x =>
            {
                x.Content = text;
                x.Embed = embed;
                x.Components = components;
            });
        }
        else await RespondAsync(text, embed: embed, components: components, ephemeral:true);
    }
    
    [ComponentInteraction("giftmenu:*")]
    public async Task GiftMenu(string _, params string[] selections)
    {
        string selection = selections.First();
        long gameId = long.Parse(selection);

        GiftCarrier? carrier = GiftService.cachedGifts.Find(x => x.GameId == gameId);
        if (carrier == null)
            return;

        var componentBuilder = new ComponentBuilder();
        
        carrier.Users.ForEach(x => componentBuilder.WithButton($"Ask {x.UserName}", $"giftget:{selection}:{x.UserId.ToString()}"));
        
        string gameCount = (carrier.Gifts.Count > 1) ? $"are {carrier.Gifts.Count} copies" : "is 1 copy";
        string text = $"There {gameCount} of the game {carrier.GameName} available.";
        if (carrier.GiftType == GiftType.Steam)
            text += "\nSteam link: " + carrier.GameText;

        await RespondAsync(text, allowedMentions: AllowedMentions.None, components: componentBuilder.Build(), ephemeral: true);
    }

    [ComponentInteraction("giftget:*:*")]
    public async Task GiftGet(string gameIdStr, string userIdStr)
    {
        ulong userId = ulong.Parse(userIdStr);
        long gameId = long.Parse(gameIdStr);
        
        GiftCarrier? carrier = GiftService.cachedGifts.Find(x => x.GameId == gameId);
        if (carrier == null)
            return;

        GiftUser? user = carrier.Users.Find(x => x.UserId == userId);
        if (user == null)
            return;

        List<GiftUserStorage> availableGifts = carrier.Gifts.Where(x => user.GiftIds.Contains(x.Id)).ToList();

        if (availableGifts.Count < 1)
            return;

        GiftUserStorage gift = availableGifts.First();

        IUser discordUser = await Context.Client.GetUserAsync(userId);
        var channel = await discordUser.CreateDMChannelAsync();

        var componentBuilder = new ComponentBuilder()
            .WithButton("Yes", $"giftaccept:{gift.Id}:{carrier.GameId}:{Context.User.Id}")
            .WithButton("No", $"giftdeny:{Context.User.Id}");

        await channel.SendMessageAsync(
            $"{Context.User.Mention} ({Context.User.Username}#{Context.User.Discriminator} from server {Context.Guild.Name}) wants to have the game {gift.GameName}. Do you want to gift this game?",
            components: componentBuilder.Build());
        
        await RespondEphemeral($"Asked {discordUser.Mention} ({discordUser.Username}#{discordUser.Discriminator}) for the game {gift.GameName}. Please have your DM's open with the bot to be able to receive a response");
    }

    [ComponentInteraction("giftdeny:*")]
    public async Task GiftDeny(string userIdStr)
    {
        ulong userId = ulong.Parse(userIdStr);
        IUser discordUser = await Context.Client.GetUserAsync(userId);
        var channel = await discordUser.CreateDMChannelAsync();
        await channel.SendMessageAsync($"{Context.User.Mention} ({Context.User.Username}#{Context.User.Discriminator}) has denied your gift");
        await RespondEphemeral($"Denied gift");
    }

    [ComponentInteraction("giftaccept:*:*:*")]
    public async Task GiftAccept(string giftIdStr, string gameIdStr, string userIdStr)
    {
        ulong userId = ulong.Parse(userIdStr);
        long gameId = long.Parse(gameIdStr);
        long giftId = long.Parse(giftIdStr);
        
        GiftCarrier? carrier = GiftService.cachedGifts.Find(x => x.GameId == gameId);
        GiftUserStorage? gift = carrier?.Gifts.Find(x => x.Id == giftId);
        
        if (gift == null)
            return;
        
        IUser discordUser = await Context.Client.GetUserAsync(userId);
        var channel = await discordUser.CreateDMChannelAsync();
        await channel.SendMessageAsync($"Key of {carrier.GameName}, gifted by {Context.User.Mention} ({Context.User.Username}#{Context.User.Discriminator}): {gift.GameKey}");
        await GiftService.RemoveKey(gift);
        await RespondEphemeral($"Accepted gift of {carrier.GameName} to {discordUser.Mention} ({discordUser.Username}#{discordUser.Discriminator}). Thanks!");
    }
}