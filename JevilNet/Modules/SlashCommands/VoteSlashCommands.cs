using Discord;
using Discord.Interactions;
using JevilNet.Modules.Base;
using JevilNet.Modules.Interactions;
using JevilNet.Services.Vote;

namespace JevilNet.Modules.SlashCommands;

[Group("vote", "Interact with the key gift system")]
[RequireContext(ContextType.Guild)]
public class VoteSlashCommands : SlashCommandBase
{
    public VoteService VoteService { get; set; }
    private IBaseInterface me => this;

    private VoteModel Model => VoteService.GetOrDefaultServerStorage(Context.Guild.Id).CustomStorage;
    private bool Active => Model.Active;

    private async Task<bool> IsOwnerOrCreator() => (Active) && ((await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner.Id == Context.User.Id ||
                                                                VoteService.IsCreator(Context.Guild.Id, Context.User.Id));

    [SlashCommand("menu", "Sends a vote menu in the current channel")]
    public async Task CreateMenu()
    {
        if (!Active)
        {
            await me.RespondEphermeral("No vote is currently active");
            return;
        }
        
        await me.Respond(Model.Name, components: VoteService.BuildMainView(Context.Guild.Id).Build());
    }

    [SlashCommand("create", "Pops up a form to create a vote")]
    public async Task CreateVote()
    {
        if (Active)
        {
            await me.RespondEphermeral("A vote is already active!");
            return;
        }
        
        await Context.Interaction.RespondWithModalAsync<VoteInteractions.VoteCreateModal>("vote_start_modal");
    }

    [SlashCommand("tally", "Shows how many votes each entry has")]
    public async Task Tally()
    {
        if (!await IsOwnerOrCreator())
        {
            await me.RespondEphermeral("You cannot do this!");
            return;
        }

        await me.RespondEphermeral(embed: VoteService.BuildTallyEmbed(Context.Guild.Id).Build());
    }

    [SlashCommand("end", "Ends the current vote and shows the tally")]
    public async Task EndVote()
    {
        if (!await IsOwnerOrCreator())
        {
            await me.RespondEphermeral("You cannot do this!");
            return;
        }
        
        await me.Respond(embed: VoteService.BuildTallyEmbed(Context.Guild.Id).Build());
        await VoteService.EndVote(Context.Guild.Id);
    }
}