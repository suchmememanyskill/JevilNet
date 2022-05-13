
using Discord;
using Discord.Commands;
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
        if (type == GiftType.Steam)
            await GiftService.AddSteamKey(me.Guild().Id, me.User().Id, me.User().Username, gameName, key);
        else
            await GiftService.AddCustomKey(me.Guild().Id, me.User().Id, me.User().Username, gameName, key);

        await Context.Message.DeleteAsync();
        await ReplyAsync("Added key");
    }

    [Command("add")]
    [Priority(1)]
    public async Task GiftAdd(GiftType type, long appId, [Remainder] string key)
    {
        if (type == GiftType.Steam)
            await GiftService.AddSteamKey(me.Guild().Id, me.User().Id, me.User().Username, appId, key);
        else
            throw new Exception("Custom keys cannot be set using an id");
        
        await Context.Message.DeleteAsync();
        await ReplyAsync("Added key");
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