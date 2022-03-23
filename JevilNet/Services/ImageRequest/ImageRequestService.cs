using System.ComponentModel;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using JevilNet.Extentions;
using JevilNet.Services.ImageRequest.Sources;
using JevilNet.Services.ImageRequest.Sources.Cade;
using JevilNet.Services.ImageRequest.Sources.E6;
using JevilNet.Services.UserSpecificGuildStorage;
using Microsoft.VisualBasic;

namespace JevilNet.Services.ImageRequest;

public class ImageRequestService : UserSpecificGuildStorage<Empty, ImageResponse>
{
    private Dictionary<long, ImageResponse> savedPosts = new();

    public async Task ProcessRequest(string source, string[] args, SocketTextChannel channel)
    {
        IImageRequest request = null;

        switch (source)
        {
            case "cat":
            case "cade":
                request = new TheCatApiSource();
                break;
        }

        if (channel.IsNsfw)
        {
            switch (source)
            {
                case "r34":
                case "rule34":
                    request = new R34ImageSource();
                    break;
                case "e6":
                case "e621":
                    request = new E6ImageSource();
                    break;
            }
        }

        if (request == null)
            throw new Exception($"No source with name `{source}` found");

        await channel.SendMessageAsync("Processing request...");
        
        List<string> tags = new();
        List<ImageRequestParameter> parameters = new();

        foreach (var str in args)
        {
            if (str.Contains("&"))
                throw new Exception("No.");
            
            if (str.Contains("="))
            {
                string[] split = str.Split("=");
                parameters.Add(new(split[0], split[1]));
            }
            else
                tags.Add(str);
        }

        List<ImageResponse> responses = await request.GetImages(tags, parameters);

        foreach (var x in responses.SplitInParts(10))
        {
            int i = 1;
            List<Embed> embeds = new();
            ComponentBuilder builder = new();
            foreach (var imageResponse in x)
            {
                long random = Program.Random.NextInt64();
                savedPosts.Add(random, imageResponse);

                builder.WithButton($"Add {i}", $"ImageAdd:{random}")
                    .WithButton($"Del {i}", $"ImageDel:{random}", ButtonStyle.Danger);
                
                embeds.Add(new EmbedBuilder().WithImageUrl(imageResponse.Url).WithTitle(i.ToString()).Build());
                i++;
            }
            
            await channel.SendMessageAsync(embeds: embeds.ToArray(), components: builder.Build());
        }
    }

    public async Task ImageAdd(ulong serverId, ulong userId, string username, long imageId)
    {
        if (!savedPosts.ContainsKey(imageId))
            throw new Exception("Image not cached. Did the bot restart?");

        ImageResponse cache = savedPosts[imageId];
        ImageResponse store = GetOrDefaultUserStorage(serverId, userId).CustomStorage.Find(x => x.Id == cache.Id);

        if (store != null)
            throw new Exception("Image has already been added in the past");
        
        await AddToUser(serverId, userId, username, cache);
    }

    public async Task ImageRemove(ulong serverId, ulong userId, long imageId)
    {
        if (!savedPosts.ContainsKey(imageId))
            throw new Exception("Image not cached. Did the bot restart?");

        ImageResponse cache = savedPosts[imageId];
        ImageResponse store = GetOrDefaultUserStorage(serverId, userId).CustomStorage.Find(x => x.Id == cache.Id);

        if (store == null)
            throw new Exception("Image is not stored!");
        
        await DelFromUser(serverId, userId, store);
    }

    public async Task CreateMenu(SocketTextChannel channel, ulong serverId, ulong userId, int page)
    {
        List<ImageResponse> responses = GetOrDefaultUserStorage(serverId, userId).CustomStorage;

        if (responses.Count == 0)
        {
            await channel.SendMessageAsync("You do not have any images stored!");
            return;
        }

        page--;

        if (page < 0 || page >= responses.Count)
        {
            await channel.SendMessageAsync("This page is out of range");
            return;
        }

        await channel.SendMessageAsync(embed: GenerateEmbed(serverId, userId, page),
            components: GenerateComponents(serverId, userId, page));
    }
    
    public async Task UpdateMenu(SocketMessageComponent interaction, ulong serverId, ulong userId, int page)
    {
        await interaction.UpdateAsync(x =>
        {
            x.Embed = GenerateEmbed(serverId, userId, page);
            x.Components = GenerateComponents(serverId, userId, page);
        });
    }
    private Embed GenerateEmbed(ulong serverId, ulong userId, int page)
    {
        List<ImageResponse> responses = GetOrDefaultUserStorage(serverId, userId).CustomStorage;

        if (page < 0 || page >= responses.Count)
            return null;

        ImageResponse image = responses[page];
        
        return new EmbedBuilder()
            .WithTitle($"Page {page + 1}/{responses.Count}")
            .WithImageUrl(image.Url)
            .WithColor(((uint)Program.Random.Next()) & 0x00FFFFFF)
            .Build();
    }

    private MessageComponent GenerateComponents(ulong serverId, ulong userId, int page)
    {
        List<ImageResponse> responses = GetOrDefaultUserStorage(serverId, userId).CustomStorage;
        long id = 0;
        
        if (page >= 0 && page < responses.Count)
        {
            id = Program.Random.NextInt64();
            savedPosts.Add(id, responses[page]);
        }

        int deletedPageNum = (page == responses.Count - 1) ? page - 1 : page;
        
        return new ComponentBuilder()
            .WithButton("Previous page", $"ImagePage:{userId}:{page - 1}", disabled: page < 1)
            .WithButton("Next page", $"ImagePage:{userId}:{page + 1}", disabled: page + 1 >= responses.Count)
            .WithButton($"Remove", $"ImageDelPage:{id}:{userId}:{deletedPageNum}", ButtonStyle.Danger)
            .Build();
    }
    

    public override string StoragePath() => "./Storage/images.json";
}