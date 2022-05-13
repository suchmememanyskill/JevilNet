
using Discord;
using Discord.Commands;
using JevilNet.Extentions;
using JevilNet.Modules.Base;
using JevilNet.Services.Gift;

namespace JevilNet.Modules.TextCommands;

[Group("gift")]
[Alias("gifts", "friigam")]
[RequireContext(ContextType.Guild)]
public class GiftTextCommands : TextCommandBase
{
    public GiftService GiftService { get; set; }
    private IBaseInterface me => this;

    [Command("add")]
    public async Task GiftAdd(GiftType type, string gameName, [Remainder] string key)
    {
        await Context.Message.DeleteAsync();
        
        if (type == GiftType.Steam)
            await GiftService.AddSteamKey(me.Guild().Id, me.User().Id, me.User().Username, gameName, key);
        else
            await GiftService.AddCustomKey(me.Guild().Id, me.User().Id, me.User().Username, gameName, key);
        
        await ReplyAsync("Added key");
    }

    [Command("add")]
    [Priority(1)]
    public async Task GiftAdd(GiftType type, long appId, [Remainder] string key)
    {
        await Context.Message.DeleteAsync();
        
        if (type == GiftType.Steam)
            await GiftService.AddSteamKey(me.Guild().Id, me.User().Id, me.User().Username, appId, key);
        else
            throw new Exception("Custom keys cannot be set using an id");
        
        await ReplyAsync("Added key");
    }

    [Command("mine")]
    [Alias("me", "self", "own")]
    public async Task GiftSelfGet()
    {
        var gifts = GiftService.GetAllGiftsOfUser(me.User().Id);
        if (gifts.Count <= 0)
        {
            await ReplyAsync("You have no gifts");
            return;
        }

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
        await me.React(Emoji.Parse(":+1:"));
    }

    [Command]
    public async Task ShowGifts()
    {
        var selectMenuBuilder = new SelectMenuBuilder()
            .WithCustomId("giftmenu")
            .WithMinValues(1)
            .WithMaxValues(1);

        if (GiftService.cachedGifts.Count <= 0)
        {
            await ReplyAsync("No gifts are available");
            return;
        }
        
        GiftService.cachedGifts.ForEach(x => selectMenuBuilder.AddOption(x.GameName, x.GameId.ToString(), $"{x.Gifts.Count} gift(s) available (Platform: {x.GiftType})"));
        
        var componentBuilder = new ComponentBuilder()
            .WithSelectMenu(selectMenuBuilder);

        await ReplyAsync("Available gifts:", components: componentBuilder.Build());
    }
}