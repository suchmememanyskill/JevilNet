using Discord;
using Discord.WebSocket;

namespace JevilNet.Services;

public record MenuStorage(int PerPage, List<string> Elements, string Name)
{
    public int PageCount => (Elements.Count + PerPage - 1) / PerPage;
    public List<string> Slice(int page) => Elements.Skip((page - 1) * PerPage).Take(PerPage).ToList();
}

public delegate Task MenuServiceSendCallback(Embed embed, MessageComponent component);

public class MenuService
{
    public Dictionary<long, MenuStorage> storage = new();

    public async Task CreateMenu(MenuServiceSendCallback callback, MenuStorage menu, int startingPage = 1)
    {
        long id = Program.Random.NextInt64();
        storage.Add(id, menu);
        await callback(GenerateEmbed(menu, startingPage), GenerateComponents(menu, startingPage, id));
    }

    public async Task UpdateMenu(SocketMessageComponent interaction, long id, int page)
    {
        if (!storage.ContainsKey(id))
            return;

        MenuStorage menu = storage[id];
        
        await interaction.UpdateAsync(x =>
        {
            x.Embed = GenerateEmbed(menu, page);
            x.Components = GenerateComponents(menu, page, id);
        });
    }

    private Embed GenerateEmbed(MenuStorage menu, int page)
    {
        return new EmbedBuilder()
            .WithTitle($"{menu.Name} (Page {page}/{menu.PageCount})")
            .WithDescription(String.Join("\n", menu.Slice(page)))
            .WithColor(((uint)Program.Random.Next()) & 0x00FFFFFF)
            .Build();
    }

    private MessageComponent GenerateComponents(MenuStorage menu, int page, long id)
    {
        return new ComponentBuilder()
            .WithButton("Previous page", $"menu:{id}:{page - 1}", disabled: page <= 1)
            .WithButton("Next page", $"menu:{id}:{page + 1}", disabled: page >= menu.PageCount)
            .Build();
    }
}