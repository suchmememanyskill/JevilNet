using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Modules.Base;
using JevilNet.Services.ImageRequest;

namespace JevilNet.Modules.Interactions;

public class ImageRequestInteractions : SlashCommandBase
{
    public ImageRequestService Service { get; set; }

    [ComponentInteraction("ImageAdd:*")]
    public async Task ImageAdd(string strId)
    {
        long id = long.Parse(strId);
        await Service.ImageAdd(Context.Guild.Id, Context.User.Id, Context.User.Username, id);
        await RespondAsync("Added Image", ephemeral: true);
    }

    [ComponentInteraction("ImageDel:*")]
    public async Task ImageDel(string strId)
    {
        long id = long.Parse(strId);
        await Service.ImageRemove(Context.Guild.Id, Context.User.Id, id);
        await RespondAsync("Removed Image", ephemeral: true);
    }

    [ComponentInteraction("ImagePage:*:*")]
    public async Task ImagePage(string strUserId, string strPage)
    {
        ulong userId = ulong.Parse(strUserId);
        int page = int.Parse(strPage);

        if (userId != Context.User.Id)
            throw new Exception("You didn't invoke this menu!");

        await Service.UpdateMenu(Context.Interaction as SocketMessageComponent, Context.Guild.Id, Context.User.Id,
            page);
    }

    [ComponentInteraction("ImageDelPage:*:*:*")]
    public async Task ImageDelPage(string strId, string strUserId, string strPage)
    {
        long id = long.Parse(strId);
        ulong userId = ulong.Parse(strUserId);

        if (userId != Context.User.Id)
            throw new Exception("You didn't invoke this menu!");
        
        await Service.ImageRemove(Context.Guild.Id, Context.User.Id, id);
        await ImagePage(strUserId, strPage);
    }
}