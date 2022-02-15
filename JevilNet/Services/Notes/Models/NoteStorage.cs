namespace JevilNet.Services.Models;

public class NoteStorage
{
    public Dictionary<ulong, List<Note>> Notes { get; set; } = new();
    public List<NoteReminder> NoteReminders { get; set; } = new();
}