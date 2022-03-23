using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using JevilNet.Modules.Base;
using JevilNet.Services.ImageRequest;

namespace JevilNet.Modules.TextCommands;

[Group("image")]
[RequireContext(ContextType.Guild)]
public class ImageRequest : TextCommandBase
{
    public ImageRequestService Service { get; set; }

    [Command]
    [Summary("Gets images from provided source")]
    public async Task GetImage(string source, params string[] parameters)
    {
        await Service.ProcessRequest(source, parameters, Context.Channel as SocketTextChannel);
    }

    [Command("saved")]
    [Alias("save", "sav", "fav")]
    [Summary("Gets your saved images")]
    public async Task GetSaved(int page = 1)
    {
        await Service.CreateMenu(Context.Channel as SocketTextChannel, Context.Guild.Id, Context.User.Id, page);
    }
}