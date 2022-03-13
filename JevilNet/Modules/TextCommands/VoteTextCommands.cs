using Discord;
using Discord.Commands;
using JevilNet.Attributes;
using JevilNet.Services.Vote;

namespace JevilNet.Modules.TextCommands;

[Group("vote")]
[Alias("votes")]
[RequireContext(ContextType.Guild)]
public class VoteTextCommands : ModuleBase<SocketCommandContext>
{
    public VoteService VoteService { get; set; }
    
    [Command]
    [Summary("Initiates the vote interaction menu")]
    public async Task InitiateVoteMenu()
    {
        if (VoteService.GetOrDefaultServerStorage(Context.Guild.Id).CustomStorage.Active)
        {
            await ReplyAsync("A vote is currently active", components: VoteService.BuildMainView(Context.Guild.Id).Build());
        }
        else
        {
            var builder = new ComponentBuilder()
                .WithButton("Create vote", "vote_start_button");

            await ReplyAsync("A vote is currently not active", components: builder.Build());
        }
    }

    [Command("start")]
    [Alias("create")]
    [VoteInactive]
    [Summary("Start a vote")]
    public async Task StartVote(string name, int min, int max, params string[] options)
    {
        if (min < 1 || max < 1)
        {
            await ReplyAsync("Invalid min and or max");
            return;
        }

        if (options.Length < 1)
        {
            await ReplyAsync("Invalid options");
            return;
        }

        await VoteService.Start(Context.Guild.Id, Context.User.Id, name, new List<string>(options), max, min);

        ComponentBuilder builder = VoteService.BuildMainView(Context.Guild.Id);
        await ReplyAsync("Created vote successfully!", components: builder.Build());
    }

    [Command("tally")]
    [VoteActive]
    [VoteCreator(Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Summary("Dms a tally of the votes to the user")]
    public async Task DmTally()
    {
        await Context.User.SendMessageAsync(embed: VoteService.BuildTallyEmbed(Context.Guild.Id).Build());
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    [Command("end")]
    [Alias("stop", "del")]
    [VoteActive]
    [VoteCreator(Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Summary("Ends a vote")]
    public async Task EndVote()
    {
        await ReplyAsync(embed: VoteService.BuildTallyEmbed(Context.Guild.Id).Build());
        await VoteService.EndVote(Context.Guild.Id);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }
}