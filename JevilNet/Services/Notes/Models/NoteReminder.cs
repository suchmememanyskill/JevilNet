namespace JevilNet.Services.Models;

public class NoteReminder
{
    public ulong MemberId { get; set; }
    public int NoteId { get; set; }
    public DateTimeOffset AlertWhen { get; set; }
    public bool DeleteNoteAfter { get; set; } = false;

    public NoteReminder()
    {
    }

    public NoteReminder(ulong memberId, int noteId, DateTimeOffset alertWhen)
    {
        MemberId = memberId;
        NoteId = noteId;
        AlertWhen = alertWhen;
    }
    
    public NoteReminder(ulong memberId, int noteId, DateTimeOffset alertWhen, bool deleteNoteAfter)
    {
        MemberId = memberId;
        NoteId = noteId;
        AlertWhen = alertWhen;
        DeleteNoteAfter = deleteNoteAfter;
    }
}