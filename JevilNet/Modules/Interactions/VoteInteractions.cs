using System.Globalization;
using Discord;
using Discord.Interactions;
using JevilNet.Attributes;
using JevilNet.Services.Vote;
using Microsoft.VisualBasic;

namespace JevilNet.Modules.Interactions;

public class VoteInteractions : InteractionModuleBase<SocketInteractionContext>
{
    public VoteService VoteService { get; set; }
    
    [ComponentInteraction("vote_start_button")]
    public async Task NoteAdd()
    {
        if (VoteService.Active)
            return;
        
        await Context.Interaction.RespondWithModalAsync<VoteCreateModal>("vote_start_modal");
    }
    
    [ModalInteraction("vote_start_modal")]
    public async Task NoteAddModal(VoteCreateModal modal)
    {
        int min = modal.MinPerPersonInt();
        int max = modal.MaxPerPersonInt();

        if (min < 1 || max < 1)
        {
            await RespondAsync("Invalid min and or max", ephemeral: true);
            return;
        }

        List<string> options = modal.Options.Split('\n').ToList();
        
        if (options.Count < 1)
        {
            await RespondAsync("Invalid options");
            return;
        }
            
        await VoteService.Start(modal.Name, options, max, min);

        ComponentBuilder builder = VoteService.BuildMainView();
        await RespondAsync("Created vote successfully!", components: builder.Build());
    }
    
    [ComponentInteraction("vote_my_votes")]
    public async Task DisplayOwnVotes()
    {
        if (!VoteService.Active)
            return;
        
        List<string> votes = VoteService.GetUserVotes(Context.User.Id);
        if (votes.Count < 1)
            await RespondAsync("You have not voted on any option", ephemeral: true);
        else
            await RespondAsync($"You have voted on the following options:\n{string.Join("\n", votes)}", ephemeral: true);
    }
    
    [ComponentInteraction("vote_delete")]
    public async Task DeleteOwnVotes()
    {
        if (!VoteService.Active)
            return;
        
        List<string> votes = VoteService.GetUserVotes(Context.User.Id);
        if (votes.Count < 1)
            await RespondAsync("You have not voted on any option", ephemeral: true);
        else
        {
            await VoteService.RemoveVote(Context.User.Id);
            await RespondAsync("Votes removed", ephemeral: true);
        }
    }

    [ComponentInteraction("vote_menu")]
    public async Task VoteMenu(params string[] args)
    {
        if (!VoteService.Active)
            return;
        
        List<int> votes = args.ToList().Select(x => int.Parse(x)).Where(x => x >= 0 && x < VoteService.Options.Count).ToList();

        if (votes.Count != args.Length || votes.Count > VoteService.MaxPerPerson ||
            votes.Count < VoteService.MinPerPerson)
        {
            await RespondAsync("Invalid vote. Are you using an old vote to vote?", ephemeral: true);
            return;
        }
        
        await VoteService.AddVote(Context.User.Id, votes);
        await RespondAsync("Successfully voted", ephemeral: true);
    }

    public class VoteCreateModal : IModal
    {
        public string Title => "Create a vote";
        [InputLabel("Name")]
        [ModalTextInput("name", placeholder: "Name of the vote", maxLength: 48)]
        public string Name { get; set; }
        
        [InputLabel("Min per person")]
        [ModalTextInput("min", placeholder: "Minimum votes per person", maxLength: 4)]
        public string MinPerPerson { get; set; }

        public int MinPerPersonInt()
        {
            if (int.TryParse(MinPerPerson, out int result))
                return result;
            
            return -1;
        }
        
        [InputLabel("Max per person")]
        [ModalTextInput("max", placeholder: "Maximum votes per person", maxLength: 4)]
        public string MaxPerPerson { get; set; }
        
        public int MaxPerPersonInt()
        {
            if (int.TryParse(MaxPerPerson, out int result))
                return result;
            
            return -1;
        }
        
        [InputLabel("Options")]
        [ModalTextInput("options", style: TextInputStyle.Paragraph, placeholder: "Specify 1 option per line", maxLength: 1000)]
        public string Options { get; set; }
    }
}