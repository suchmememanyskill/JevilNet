using System.Reflection;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Services.Help.Models;
using Microsoft.Extensions.Configuration;

namespace JevilNet.Services;

public class TextHelp
{
    public List<TextModuleHelp> Modules { get; private set; }
    
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionCommands;
    private readonly CommandService _textCommands;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;

    public TextHelp(DiscordSocketClient client, CommandService textCommands,
        InteractionService interactionCommands, IServiceProvider services, IConfiguration config)
    {
        _client = client;
        _interactionCommands = interactionCommands;
        _services = services;
        _textCommands = textCommands;
        _config = config;
    }

    public void Initialise()
    {
        Modules = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.Namespace.StartsWith("JevilNet.Modules.TextCommands")).Select(x => new TextModuleHelp(x)).Where(x => x.Commands.Count > 0).ToList();
    }

    public string GenerateHelp() => string.Join("\n\n", Modules);
}