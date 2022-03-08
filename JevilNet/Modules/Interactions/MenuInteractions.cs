using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Services;

namespace JevilNet.Modules.Interactions;

public class MenuInteractions : InteractionModuleBase<SocketInteractionContext>
{
    public MenuService Menu { get; set; }
    
    [ComponentInteraction("menu:*:*")]
    public async Task MenuCall(string menuId, string page)
    {
        if (Context.Interaction is SocketMessageComponent interaction)
            await Menu.UpdateMenu(interaction, long.Parse(menuId), int.Parse(page));
    }
}