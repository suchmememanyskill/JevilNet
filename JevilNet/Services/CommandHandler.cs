using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ExecuteResult = Discord.Commands.ExecuteResult;
using PreconditionResult = Discord.Commands.PreconditionResult;

namespace JevilNet.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionCommands;
    private readonly CommandService _textCommands;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;

    public CommandHandler(DiscordSocketClient client, CommandService textCommands,
        InteractionService interactionCommands, IServiceProvider services, IConfiguration config)
    {
        _client = client;
        _interactionCommands = interactionCommands;
        _services = services;
        _textCommands = textCommands;
        _config = config;
    }

    public async Task InitializeAsync()
    {
        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _interactionCommands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        // Another approach to get the assembly of a specific type is:
        // typeof(CommandHandler).Assembly

        // Register modules that are public and inherit ModuleBase<T>.
        await _textCommands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);


        // Process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;

        // Process the command execution results
        _interactionCommands.SlashCommandExecuted += SlashInteractionCommandExecuted;
        _interactionCommands.ContextCommandExecuted += ContextInteractionCommandExecuted;
        _interactionCommands.ComponentCommandExecuted += ComponentInteractionCommandExecuted;

        // Hook CommandExecuted to handle post-command-execution logic.
        _textCommands.CommandExecuted += CommandExecutedAsync;
        // Hook MessageReceived so we can process each message to see
        // if it qualifies as a command.
        _client.MessageReceived += MessageReceivedAsync;
    }

    public async Task DeInitialiseAsync()
    {
        _client.InteractionCreated -= HandleInteraction;
        _interactionCommands.SlashCommandExecuted -= SlashInteractionCommandExecuted;
        _interactionCommands.ContextCommandExecuted -= ContextInteractionCommandExecuted;
        _interactionCommands.ComponentCommandExecuted -= ComponentInteractionCommandExecuted;
        _textCommands.CommandExecuted -= CommandExecutedAsync;
        _client.MessageReceived -= MessageReceivedAsync;
    }

    # region Interaction Error Handling

    private async Task ComponentInteractionCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2,
        Discord.Interactions.IResult arg3)
    {
        Console.WriteLine($"{DateTime.Now}: {arg2.User.Username} executed interaction {arg1.Name}");
        
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    if (arg2 is SocketInteractionContext ctx)
                    {
                        await ctx.Interaction.RespondAsync("You are missing permissions to do this", ephemeral:true);
                    }
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    if (arg3 is Discord.Interactions.ExecuteResult execResult)
                    {
                        if (arg2 is SocketInteractionContext ctx2)
                        {
                            await ctx2.Interaction.RespondAsync(execResult.Exception.Message, ephemeral:true);
                            return;
                        }
                    }
                    break;
                case InteractionCommandError.Unsuccessful:
                    // implement
                    break;
                default:
                    break;
            }
        }
    }

    private Task ContextInteractionCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2,
        Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    // implement
                    break;
                case InteractionCommandError.Unsuccessful:
                    // implement
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private async Task SlashInteractionCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2,
        Discord.Interactions.IResult arg3)
    {
        Console.WriteLine($"{DateTime.Now}: {arg2.User.Username} executed slash command {arg1.Name}");
        
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    if (arg2 is SocketInteractionContext ctx)
                    {
                        await ctx.Interaction.RespondAsync("You are missing permissions to do this", ephemeral:true);
                    }
                    // implement
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    if (arg3 is Discord.Interactions.ExecuteResult execResult)
                    {
                        if (arg2 is SocketInteractionContext ctx2)
                        {
                            await ctx2.Interaction.RespondAsync(execResult.Exception.Message, ephemeral:true);
                            return;
                        }
                    }
                    goto default;
                case InteractionCommandError.Unsuccessful:
                    // implement
                    break;
                default:
                    break;
            }
        }
    }

    # endregion

    # region Interaction Execution

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_client, arg);
            await _interactionCommands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    # endregion

    #region Message command handler
    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (!(rawMessage is SocketUserMessage message))
            return;
        if (message.Source != MessageSource.User)
            return;
        

        if (rawMessage.Channel is SocketDMChannel dmChannel)
        {
            IChannel channel = await _client.GetChannelAsync(_config.GetValue<ulong>("logChannel"));
            string text =
                $"```\n[DM] <{rawMessage.Author.Username}> {rawMessage.Content.Replace("```", "'''")}\n```";
            if (channel is ITextChannel textChannel)
                await textChannel.SendMessageAsync(text);
            
            Console.WriteLine(text);
        }
        
        // This value holds the offset where the prefix ends
        var argPos = 0;
        // Perform prefix check. You may want to replace this with
        // (!message.HasCharPrefix('!', ref argPos))
        // for a more traditional command format like !help.
        if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
        {
            if (!message.HasStringPrefix(_config.GetValue<string>("prefix"), ref argPos))
                return;
        }
            

        var context = new SocketCommandContext(_client, message);
        // Perform the execution of the command. In this method,
        // the command service will perform precondition and parsing check
        // then execute the command if one is matched.
        await _textCommands.ExecuteAsync(context, argPos, _services);
        // Note that normally a result will be returned by this format, but here
        // we will handle the result in CommandExecutedAsync,
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
    {
        // command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;
        
        Console.WriteLine($"{DateTime.Now}: {context.User.Username} executed text command {command.Value.Aliases.First()}");

        // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
            return;

        switch (result.Error)
        {
            case CommandError.ParseFailed:
            case CommandError.BadArgCount:
                var cmd = command.Value;
                var parameters = cmd.Parameters.Select(p => string.IsNullOrEmpty(p.Summary) ? p.Name : p.Summary);
                var paramsString = $"Parameters: {string.Join(", ", parameters)}" +
                                   (string.IsNullOrEmpty(cmd.Summary) ? "" : $"\nSummary: {cmd.Summary}") +
                                   (string.IsNullOrEmpty(cmd.Remarks) ? "" : $"\nRemarks: {cmd.Remarks}");
                
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Title = $"Help for {string.Join(", ", cmd.Aliases)}",
                    Description = paramsString
                };
                
                await context.Channel.SendMessageAsync("Invalid command usage!", embed: builder.Build());
                break;
            
            case CommandError.Exception:
                if (result is ExecuteResult execResult)
                {
                    await context.Channel.SendMessageAsync(execResult.Exception.Message,
                        allowedMentions: AllowedMentions.None);
                    return;
                }
                goto default;
            case CommandError.UnmetPrecondition:
                if (result is PreconditionResult preResult)
                {
                    await context.Channel.SendMessageAsync(preResult.ErrorReason,
                        allowedMentions: AllowedMentions.None);
                    return;
                }
                goto default;
            default:
                // the command failed, let's notify the user that something happened.
                await context.Channel.SendMessageAsync($"error: {result}", allowedMentions: AllowedMentions.None);
                break;
        }
    }
    #endregion
}