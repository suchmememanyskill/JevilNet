using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using JevilNet.Extentions;
using JevilNet.Services;
using JevilNet.Services.CustomCommand;
using JevilNet.Services.Gift;
using JevilNet.Services.ImageRequest;
using JevilNet.Services.Quote;
using JevilNet.Services.Roles;
using JevilNet.Services.Vote;

namespace JevilNet;


class Program
{
    // Entry point of the program.
    static void Main ( string[] args )
    {
        // One of the more flexable ways to access the configuration data is to use the Microsoft's Configuration model,
        // this way we can avoid hard coding the environment secrets. I opted to use the Json and environment variable providers here.
        IConfiguration config = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "DC_")
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        List<string> required = new()
        {
            "prefix",
            "token",
            "testServer",
            "logChannel",
        };
        if (config.GetChildren().Count(x => required.Contains(x.Key)) != required.Count())
            throw new Exception("Missing configuration...");
            
        RunAsync(config).GetAwaiter().GetResult();
    }

    public static ServiceProvider? staticServices;
    public static Random Random = new Random();

    static async Task RunAsync (IConfiguration configuration)
    {
        // Dependency injection is a key part of the Interactions framework but it needs to be disposed at the end of the app's lifetime.
        using var services = ConfigureServices(configuration);
        staticServices = services;

        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<InteractionService>();

        client.Log += LogAsync;
        commands.Log += LogAsync;
        services.GetRequiredService<CommandService>().Log += LogAsync;
        // Slash Commands and Context Commands are can be automatically registered, but this process needs to happen after the client enters the READY state.
        // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands. To determine the method we should
        // register the commands with, we can check whether we are in a DEBUG environment and if we are, we can register the commands to a predetermined test guild.
        client.Ready += async ( ) =>
        {
            if (IsDebug())
            {
                // Id of the test guild can be provided from the Configuration object
                await commands.RegisterCommandsToGuildAsync(configuration.GetValue<ulong>("testServer"), true);
                Console.WriteLine("Setting commands on debug server...");
            }
            else
            {
                await commands.RegisterCommandsGloballyAsync(true);
                Console.WriteLine("Setting commands on all servers...");
            }

            string startMessage =
                $"----------\nLogged in as:\n{client.CurrentUser.Username}\n{client.CurrentUser.Id}\n----------\n";

            startMessage += $"Currently joined servers:\n{string.Join('\n', client.Guilds.Select(x => x.Name))}";

            IChannel c = await client.GetChannelAsync(configuration.GetValue<ulong>("logChannel"));
            if (c is ITextChannel d)
                await d.SendBlockAsync(startMessage);
        };

        // Here we can initialize the service that will register and execute our commands
        await services.GetRequiredService<CommandHandler>().InitializeAsync();

        // Bot token can be provided from the Configuration object we set up earlier
        await client.LoginAsync(TokenType.Bot, configuration["token"]);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    static Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    static ServiceProvider ConfigureServices ( IConfiguration configuration )
        => new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            }))
            .AddSingleton<EmoteService>()
            .AddSingleton<CommandService>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandler>()
            .AddSingleton(x => new NoteService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<VoteService>()
            .AddSingleton<QuoteService>()
            .AddSingleton<RoleService>()
            .AddSingleton<MenuService>()
            .AddSingleton<CustomCommandService>()
            .AddSingleton<ImageRequestService>()
            .AddSingleton<ArbitraryEditService>()
            .AddSingleton<GiftService>()
            .BuildServiceProvider();

    static bool IsDebug ( )
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}