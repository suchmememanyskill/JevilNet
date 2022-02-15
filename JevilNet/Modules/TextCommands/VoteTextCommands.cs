using Discord;
using Discord.Commands;
using JevilNet.Attributes;
using JevilNet.Services.Vote;

namespace JevilNet.Modules.TextCommands;

[Group("vote")]
[Alias("votes")]
public class VoteTextCommands : ModuleBase<SocketCommandContext>
{
    public VoteService VoteService { get; set; }
    
    [Command]
    [Summary("Initiates the vote interaction menu")]
    public async Task InitiateVoteMenu()
    {
        if (VoteService.Active)
        {
            await ReplyAsync("A vote is currently active", components: VoteService.BuildMainView().Build());
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

        await VoteService.Start(name, new List<string>(options), max, min);

        ComponentBuilder builder = VoteService.BuildMainView();
        await ReplyAsync("Created vote successfully!", components: builder.Build());
    }

    [Command("tally")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [VoteActive]
    [Summary("Dms a tally of the votes to the user.")]
    public async Task DmTally()
    {
        await Context.User.SendMessageAsync(embed: VoteService.BuildTallyEmbed().Build());
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }

    [Command("end")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [VoteActive]
    [Summary("Ends a vote")]
    public async Task EndVote()
    {
        await ReplyAsync(embed: VoteService.BuildTallyEmbed().Build());;
        await VoteService.EndVote();
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }
}