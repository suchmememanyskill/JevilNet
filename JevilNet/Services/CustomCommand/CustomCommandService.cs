using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JevilNet.Services.UserSpecificGuildStorage;
using Microsoft.Extensions.Configuration;

namespace JevilNet.Services.CustomCommand;

public class CustomCommandService : UserSpecificGuildStorage<Empty, CommandStorage>
{
    public override string StoragePath() => "./Storage/commands.json";
    private DiscordSocketClient client;
    private IConfiguration config;

    public CustomCommandService(DiscordSocketClient client, IConfiguration config)
    {
        Load();
        this.client = client;
        this.config = config;
        client.MessageReceived += MessageReceivedAsync;
    }

    public async Task AddCommand(ulong serverId, ulong userId, string userName, string caller, string output)
    {
        if (GetOrDefaultServerStorage(serverId).GetCombinedStorage().Any(x => x.Caller == caller))
            throw new Exception("Command already exists!");

        if (caller.Length > 50)
            throw new Exception("Command name is too long!");
        
        await AddToUser(serverId, userId, userName, new CommandStorage(caller, output));
    }

    public async Task RemoveCommand(ulong serverId, ulong userId, string caller)
    {
        if (GetOrDefaultUserStorage(serverId, userId).CustomStorage.RemoveAll(x => x.Caller == caller) > 0)
            await Save();
        else
            throw new Exception("No command found");
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (!(rawMessage is SocketUserMessage message))
            return;
        if (message.Source != MessageSource.User)
            return;
        
        // This value holds the offset where the prefix ends
        var argPos = 0;
        // Perform prefix check. You may want to replace this with
        // (!message.HasCharPrefix('!', ref argPos))
        // for a more traditional command format like !help.
        if (!message.HasMentionPrefix(client.CurrentUser, ref argPos))
        {
            if (!message.HasStringPrefix(config.GetValue<string>("prefix"), ref argPos))
                return;
        }

        var command = message.Content.Substring(argPos).Split(" ").First();

        IChannel channel = rawMessage.Channel;
        if (channel is SocketTextChannel textChannel)
        {
            CommandStorage? commandStorage = GetOrDefaultServerStorage(textChannel.Guild.Id).GetCombinedStorage()
                .Find(x => x.Caller == command);

            if (commandStorage != null)
                await textChannel.SendMessageAsync(commandStorage.Output, allowedMentions: AllowedMentions.None);
        }
    }
}