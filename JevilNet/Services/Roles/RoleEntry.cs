namespace JevilNet.Services.Roles;

public class RoleEntry
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ulong Id { get; set; }

    public RoleEntry(string name, string description, ulong id)
    {
        Name = name;
        Description = description;
        Id = id;
    }

    public RoleEntry()
    {
    }
}