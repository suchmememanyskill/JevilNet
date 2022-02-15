using Discord;

namespace JevilNet.Services.Vote;

public class VoteService : BaseService<VoteModel>
{
    public record VoteTally(string option, int count);

    public VoteService() => Load();
    public override string StoragePath() => "./Storage/vote.json";

    public bool Active
    {
        get => storage.Active;
    }

    public string Name
    {
        get => storage.Name;
    }

    public List<string> Options
    {
        get => storage.Options;
    }
    
    public int MaxPerPerson
    {
        get => storage.MaxPerPerson;
    }
    
    public int MinPerPerson
    {
        get => storage.MinPerPerson;
    }

    public async Task Start(string name, List<string> options, int maxPerPerson, int minPerPerson)
    {
        if (maxPerPerson > options.Count)
            maxPerPerson = options.Count;

        if (minPerPerson < 1)
            minPerPerson = 1;

        if (minPerPerson > maxPerPerson)
            minPerPerson = maxPerPerson;

        if (options.Count < 1)
            throw new Exception("No vote options!");

        storage.Name = name;
        storage.Options = options;
        storage.MaxPerPerson = maxPerPerson;
        storage.MinPerPerson = minPerPerson;
        storage.UserVotes.Clear();
        storage.Active = true;
        await Save();
    }

    public async Task AddVote(ulong memberId, List<int> indexes)
    {
        storage.UserVotes[memberId] = indexes;
        await Save();
    }

    public async Task RemoveVote(ulong memberId)
    {
        storage.UserVotes.Remove(memberId);
        await Save();
    }

    public List<string> GetUserVotes(ulong memberId) => (storage.UserVotes.ContainsKey(memberId))
        ? storage.UserVotes[memberId].Select(x => Options[x]).ToList()
        : new();

    public async Task EndVote()
    {
        storage.Active = false;
        await Save();
    }
    
    public List<VoteTally> Tally()
    {
        Dictionary<int, int> localTally = new();
        foreach (var (key, value) in storage.UserVotes)
        {
            value.ForEach(x =>
            {
                if (!localTally.ContainsKey(x))
                    localTally[x] = 0;

                localTally[x]++;
            });
        }

        List<VoteTally> tally = new();
        foreach (var (key, value) in localTally)
        {
            tally.Add(new(Options[key], value));
        }

        return tally.OrderByDescending(x => x.count).ToList();
    }

    public ComponentBuilder BuildMainView()
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Vote on an option!")
            .WithCustomId("vote_menu")
            .WithMinValues(storage.MinPerPerson)
            .WithMaxValues(storage.MaxPerPerson);

        int i = 0;
        Options.ForEach(x => menuBuilder.AddOption(x, i++.ToString()));

        return new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .WithButton("My votes", "vote_my_votes")
            .WithButton("Delete vote", "vote_delete", style: ButtonStyle.Danger);
    }

    public EmbedBuilder BuildTallyEmbed() => new EmbedBuilder()
        .WithTitle(storage.Name)
        .WithDescription(string.Join("\n", Tally().Select(x => $"{x.count} votes on {x.option}")))
        .WithColor(Color.Magenta);
}