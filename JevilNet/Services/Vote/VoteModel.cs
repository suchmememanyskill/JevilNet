namespace JevilNet.Services.Vote;

public class VoteModel
{
    public string Name { get; set; }
    public List<string> Options { get; set; } = new();
    public Dictionary<ulong, List<int>> UserVotes { get; set; } = new();
    public int MaxPerPerson { get; set; }
    public int MinPerPerson { get; set; }
    public bool Active { get; set; } = false;
    public ulong CreatorId { get; set; } = 0;
}