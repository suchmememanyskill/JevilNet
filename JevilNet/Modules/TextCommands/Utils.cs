using System.ComponentModel;
using System.Diagnostics;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JevilNet.Attributes;
using JevilNet.Services;

namespace JevilNet.Modules.TextCommands;

public class Utils : ModuleBase<SocketCommandContext>
{
    public DiscordSocketClient Client { get; set; }
    public CommandHandler Handler { get; set; }
    
    public EmoteService Emote { get; set; }

    [Command("emoji")]
    [Alias("emote")]
    [Summary("Sends an emote of your choosing")]
    public async Task SendEmote(string emote)
    {
        GuildEmote? guildEmote = Emote.FindEmote(emote);
        if (guildEmote != null)
            await ReplyAsync(guildEmote.ToString());
        else
            await ReplyAsync("Didn't find the specified emote");
    }

    [Command("ping")]
    [Alias("pong", "hello")]
    [Summary("Returns pong")]
    public Task PingAsync()
        => ReplyAsync("pong!");

    [Command("stop")]
    [Alias("kill")]
    [Summary("Stops the bot")]
    [RequireOwner]
    public async Task StopBot()
    {
        await ReplyAsync("Cya!");
        await Task.Delay(1000);
        await Handler.DeInitialiseAsync();
        await Context.Client.StopAsync();
        new Thread(() => // How am i supposed to shutdown normally?
        {
            Thread.Sleep(5000);
            Environment.Exit(0);
        }).Start();
        await Context.Client.LogoutAsync();
    }
    
    [Command("source")]
    [Summary("Gives a link to the source code of this bot")]
    public Task Source() => ReplyAsync("https://github.com/suchmememanyskill/JevilNet");

    [Command("say")]
    [Summary("Sends a message on the bots behalf. Put in a channel id to send in that specific channel")]
    public async Task Say([Remainder] string message)
    {
        string[] split = message.Split(" ", 2);
        if (ulong.TryParse(split[0], out ulong result))
        {
            var channel = await Client.GetChannelAsync(result);
            if (channel is ITextChannel textChannel)
            {
                await textChannel.SendMessageAsync(split[1], allowedMentions: AllowedMentions.None);
                await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
                return;
            }
        }
        
        await ReplyAsync(message, allowedMentions: AllowedMentions.None);
        await Context.Message.DeleteAsync();
    }

    [Command("dmid")]
    [Summary("Dms a user a message via a user id")]
    public async Task DmId(ulong userId, [Remainder] string message)
    {
        var user = await Client.GetUserAsync(userId);
        var dmChannel = await user.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync(message);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    [Command("game")]
    [Summary("Sets the playing text on the bot")]
    public async Task SetGame([Remainder] string game = "")
    {
        await Client.SetGameAsync(game);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    [Command("avatar")]
    [Summary("Gets a users avatar")]
    public async Task GetAvatar(ulong id)
        => await GetAvatar(await Client.GetUserAsync(id));
    
    [Command("avatar")]
    [Summary("Gets a users avatar")]
    public async Task GetAvatar(IUser? user = null)
    {
        string url;
        user ??= Context.User;

        if (user.AvatarId.StartsWith("a_"))
            url = user.GetAvatarUrl(ImageFormat.Gif, 512);
        else
            url = user.GetAvatarUrl(ImageFormat.Png, 512);

        EmbedBuilder builder = new EmbedBuilder()
            .WithImageUrl(url)
            .WithTitle($"{user.Username}'s Avatar")
            .WithColor(((uint)Program.Random.Next()) & 0x00FFFFFF);
        
        await ReplyAsync(embed: builder.Build());
    }

    private static readonly Dictionary<string, string> validDelugeLocations = new Dictionary<string, string>()
    {
        {"movies", "/media/external_storage/jellyfin/Movies"},
        {"music",  "/media/external_storage/jellyfin/Music"},
        {"shows",  "/media/external_storage/jellyfin/Shows"},
        //{"other",  "/media/external_storage/jellyfin/Other"},
    };
    
    [Command("delugestart")]
    [Summary("Starts a deluge download")]
    [RequireOwner]
    public async Task DelugeStart(string location, [Remainder] string magnetUrl)
    {
        location = location.ToLower();

        if (!validDelugeLocations.ContainsKey(location))
        {
            await ReplyAsync(
                $"Invalid location! Valid locations are:\n- {string.Join("\n- ", validDelugeLocations.Keys.ToList())}");
            return;
        }

        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = "deluge-console";
        p.StartInfo.Arguments = $"add -p {validDelugeLocations[location]} {magnetUrl}";

        p.Start();
        string output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();
        
        Console.WriteLine(output);

        if (output.Trim() != "Torrent added!")
        {
            await ReplyAsync($"Adding url failed! Output: {output}", allowedMentions: AllowedMentions.None);
            return;
        }

        await ReplyAsync("Successfully added!");
    }

    [Command("delugestatus")]
    [Summary("Gets the in progress downloads for deluge")]
    [RequireOwner]
    public async Task DelugeStatus()
    {
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = "deluge-console";
        p.StartInfo.Arguments = "info";
        
        p.Start();
        string output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();

        List<string> items = new();
        string name = null;
        
        foreach (var s in output.Split('\n'))
        {
            if (s.StartsWith("Name"))
                name = s.Substring(6);
            else if (s.StartsWith("State"))
            {
                if (!s.Substring(7).StartsWith("Downloading")) 
                    name = null;
            }
            else if (s.StartsWith("Size") && name != null)
            {
                items.Add($"{name}: {s.Substring(6).Split("Ratio").First()}");
            }
        }
        
        if (items.Count <= 0)
            items.Add("No downloads found");

        await ReplyAsync(string.Join("\n", items), allowedMentions: AllowedMentions.None);
    }

    [Command("dndcalc")]
    [Summary("Calculate stats for a dnd encounter")]
    public async Task GenDndEncounter(int amount, int dice_count, int dice_value, int additional_hp, int initiative_bonus)
    {
        Random r = new();

        List<Tuple<int, int>> gens = new();

        for (int i = 0; i < amount; i++)
        {
            int x = 0;
            for (int j = 0; j < dice_count; j++)
                x += r.Next(1, dice_value + 1);

            x += additional_hp;

            int init = r.Next(1, 21) + initiative_bonus;
            gens.Add(new(x, init));
        }

        await ReplyAsync(string.Join("\n", gens.OrderByDescending(x => x.Item2).Select((x, i) => $"#{i + 1}: {x.Item1} HP, {x.Item2} Initiative")));
    }
}