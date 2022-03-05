namespace JevilNet.Services.Roles;

public class RoleSet
{
    public int Id { get; set; } = Program.Random.Next();
    public string SetName { get; set; }
    public List<RoleEntry> Roles { get; set; } = new();

    public RoleSet()
    {
    }

    public RoleSet(string setName)
    {
        SetName = setName;
    }
}