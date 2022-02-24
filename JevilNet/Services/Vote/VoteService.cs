using Discord;
using JevilNet.Services.UserSpecificGuildStorage;

namespace JevilNet.Services.Vote;

public class VoteService : UserSpecificGuildStorage<VoteModel, int>
{
    public record VoteTally(string option, int count);

    public VoteService() => Load();
    public override string StoragePath() => "./Storage/vote.json";
    
    public bool IsCreator(ulong guildId, ulong userId) => GetOrDefaultServerStorage(guildId).CustomStorage.CreatorId == userId;

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
        model.Active = true;
        model.CreatorId = creatorId;

        var serverStorage = GetOrDefaultServerStorage(guildId);
        serverStorage.CustomStorage = model;
        serverStorage.UserStorage.Clear();
        
        if (!storage.Contains(serverStorage))
            storage.Add(serverStorage);

        await Save();
    }

    public async Task AddVote(ulong guildId, ulong memberId, List<int> indexes)
        => await SetOnUser(guildId, memberId, "", indexes);

    public async Task RemoveVote(ulong guildId, ulong memberId)
        => await DelUser(guildId, memberId);

    public List<string> GetUserVotes(ulong guildId, ulong memberId)
    {
        var server = GetOrDefaultServerStorage(guildId);
        return GetOrDefaultUserStorage(guildId, memberId).CustomStorage.Select(x => server.CustomStorage.Options[x]).ToList();
    }

    public async Task EndVote(ulong guildId)
        => await DelServer(guildId);

    public List<VoteTally> Tally(ulong guildId)
    {
        var server = GetOrDefaultServerStorage(guildId);
        VoteModel model = server.CustomStorage;
        Dictionary<int, int> localTally = new();
        
        server.GetCombinedStorage().ForEach(x =>
        {
            if (!localTally.ContainsKey(x))
                localTally[x] = 0;

            localTally[x]++;
        });
        
        List<VoteTally> tally = new();
        foreach (var (key, value) in localTally)
        {
            tally.Add(new(model.Options[key], value));
        }

        return tally.OrderByDescending(x => x.count).ToList();
    }

    public ComponentBuilder BuildMainView(ulong guildId)
    {
        var server = GetOrDefaultServerStorage(guildId);
        VoteModel model = server.CustomStorage;
        
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
        .WithTitle(GetOrDefaultServerStorage(guildId).CustomStorage.Name)
        .WithDescription(string.Join("\n", Tally(guildId).Select(x => $"{x.count} votes on {x.option}")))
        .WithColor(Color.Magenta);
}