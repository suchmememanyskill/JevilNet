using Discord;

namespace JevilNet.Services.Vote;

public class VoteService : BaseService<VoteModelHolder>
{
    public record VoteTally(string option, int count);

    public VoteService() => Load();
    public override string StoragePath() => "./Storage/vote.json";

    public VoteModel GetModel(ulong guildId) => storage.VoteHolder.ContainsKey(guildId) ? storage.VoteHolder[guildId] : new();

    public bool IsCreator(ulong guildId, ulong userId) => GetModel(guildId).CreatorId == userId;
    public async Task SetModel(ulong guildId, VoteModel model)
    {
        storage.VoteHolder[guildId] = model;
        await Save();
    }
    
    public async Task Start(ulong guildId, ulong creatorId, string name, List<string> options, int maxPerPerson, int minPerPerson)
    {
        if (maxPerPerson > options.Count)
            maxPerPerson = options.Count;

        if (minPerPerson < 1)
            minPerPerson = 1;

        if (minPerPerson > maxPerPerson)
            minPerPerson = maxPerPerson;

        if (options.Count < 1)
            throw new Exception("No vote options!");

        VoteModel model = new();

        model.Name = name;
        model.Options = options;
        model.MaxPerPerson = maxPerPerson;
        model.MinPerPerson = minPerPerson;
        model.UserVotes.Clear();
        model.Active = true;
        model.CreatorId = creatorId;
        await SetModel(guildId, model);
    }

    public async Task AddVote(ulong guildId, ulong memberId, List<int> indexes)
    {
        GetModel(guildId).UserVotes[memberId] = indexes;
        await Save();
    }

    public async Task RemoveVote(ulong guildId, ulong memberId)
    {
        GetModel(guildId).UserVotes.Remove(memberId);
        await Save();
    }

    public List<string> GetUserVotes(ulong guildId, ulong memberId) => (GetModel(guildId).UserVotes.ContainsKey(memberId))
        ? GetModel(guildId).UserVotes[memberId].Select(x => GetModel(guildId).Options[x]).ToList()
        : new();

    public async Task EndVote(ulong guildId)
    {
        if (storage.VoteHolder.Remove(guildId))
            await Save();
    }

    public List<VoteTally> Tally(ulong guildId)
    {
        VoteModel model = GetModel(guildId);
        Dictionary<int, int> localTally = new();
        foreach (var (key, value) in model.UserVotes)
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
            tally.Add(new(model.Options[key], value));
        }

        return tally.OrderByDescending(x => x.count).ToList();
    }

    public ComponentBuilder BuildMainView(ulong guildId)
    {
        VoteModel model = GetModel(guildId);
        
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Vote on an option!")
            .WithCustomId("vote_menu")
            .WithMinValues(model.MinPerPerson)
            .WithMaxValues(model.MaxPerPerson);

        int i = 0;
        model.Options.ForEach(x => menuBuilder.AddOption(x, i++.ToString()));

        return new ComponentBuilder()
            .WithSelectMenu(menuBuilder)
            .WithButton("My votes", "vote_my_votes")
            .WithButton("Delete vote", "vote_delete", style: ButtonStyle.Danger);
    }

    public EmbedBuilder BuildTallyEmbed(ulong guildId) => new EmbedBuilder()
        .WithTitle(GetModel(guildId).Name)
        .WithDescription(string.Join("\n", Tally(guildId).Select(x => $"{x.count} votes on {x.option}")))
        .WithColor(Color.Magenta);
}