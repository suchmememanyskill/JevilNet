namespace JevilNet.Services.Models;

public class Note
{
    private static Random random = new Random();
    
    public int Id { get; set; }
    public string Name { get; set; }
    public string Contents { get; set; }

    public Note()
    {
        Id = random.Next();
    }

    public Note(string name, string contents) : this()
    {
        Name = name;
        Contents = contents;
    }
}