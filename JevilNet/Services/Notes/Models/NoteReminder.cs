namespace JevilNet.Services.Models;

public class NoteReminder
{
    public ulong MemberId { get; set; }
    public int NoteId { get; set; }
    public DateTimeOffset AlertWhen { get; set; }

    public NoteReminder()
    {
    }

    public NoteReminder(ulong memberId, int noteId, DateTimeOffset alertWhen)
    {
        MemberId = memberId;
        NoteId = noteId;
        AlertWhen = alertWhen;
    }
}