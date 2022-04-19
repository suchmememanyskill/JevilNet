using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using JevilNet.Modules.TextCommands;

namespace JevilNet.Services;

public class ArbitraryEditService : BaseService<Dictionary<ulong, Dictionary<ulong, string>>>
{
    public override string StoragePath() => "./Storage/webhooks.json";

    public ArbitraryEditService() => Load();

    private async Task<IMessage?> GetLastMessage(ITextChannel channel)
    {
        return (await channel.GetMessagesAsync(1).FlattenAsync())?.First() ?? null;
    }

    private async Task SendWebhook(string webhookUrl, string username, string avatarUrl, string content)
    {
        var hook = new DiscordWebhookClient(webhookUrl);
        await hook.SendMessageAsync(content, username: username, avatarUrl: avatarUrl, allowedMentions: AllowedMentions.None);
    }

    public async Task Edit(ITextChannel channel, string newMessage)
    {
        IMessage? message = await GetLastMessage(channel);

        if (message == null)
            throw new Exception("This channel has no messages");
        
        SocketGuildUser? user = message.Author as SocketGuildUser;

        if (user == null)
            throw new Exception("Could not fetch user info");

        if (!storage.ContainsKey(channel.Guild.Id))
            throw new Exception("No webhook set");

        var serverStorage = storage[channel.Guild.Id];
        if (!serverStorage.ContainsKey(channel.Id))
            throw new Exception("No webhook set");

        await message.DeleteAsync();
        await SendWebhook(serverStorage[channel.Id], user!.Nickname ?? user.Username, user.GetAvatarUrl(),
            newMessage);
    }

    public async Task SetWebhook(ulong serverId, ulong channelId, string? url = null)
    {
        if (!storage.ContainsKey(serverId))
            storage.Add(serverId, new Dictionary<ulong, string>());

        var serverStorage = storage[serverId];
        if (url == null)
        {
            if (serverStorage.ContainsKey(channelId))
                serverStorage.Remove(channelId);
        }
        else
        {
            serverStorage[channelId] = url;
        }

        await Save();
    }
}