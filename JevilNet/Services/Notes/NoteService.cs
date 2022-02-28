using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Services.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace JevilNet.Services;

public class NoteService : BaseService<NoteStorage>
{
    private DiscordSocketClient client;
    private Timer timer;

    public NoteService(DiscordSocketClient client)
    {
        this.client = client;
        timer = new(SearchForExpiredReminders, null, 60 * 1000, 60 * 1000);
        Load();
    }
    public override string StoragePath() => "./Storage/notes.json";

    public async Task<Note> Add(ulong memberId, string name, string contents)
    {
        if (!storage.Notes.ContainsKey(memberId))
            storage.Notes.Add(memberId, new());

        Note newNote = new Note(name, contents);
        storage.Notes[memberId].Add(newNote);
        await Save();
        return newNote;
    }

    public async Task<NoteReminder?> AddReminder(ulong memberId, int noteId, int minutesFromNow, bool deleteNoteAfter = false)
    {
        await RemoveReminder(memberId, noteId);
        
        if (minutesFromNow > 0)
        {
            NoteReminder reminder = new NoteReminder(memberId, noteId, DateTime.Now.AddMinutes(minutesFromNow), deleteNoteAfter);
            storage.NoteReminders.Add(reminder);
            await Save();
            return reminder;
        }

        return null;
    }

    public async Task RemoveReminder(ulong memberId, int noteId)
    {
        if (storage.NoteReminders.RemoveAll(x => x.NoteId == noteId && x.MemberId == memberId) > 0)
            await Save();
    }
    
    public async Task Edit(ulong memberId, int noteId, string name, string contents)
    {
        Note? note = GetNote(memberId, noteId);
        if (note == null) 
            return;
        
        note.Name = name;
        note.Contents = contents;
        await Save();
    }

    public async Task Delete(ulong memberId, int noteId)
    {
        GetNotes(memberId).RemoveAll(x => x.Id == noteId);
        await Save();
    }

    public List<Note> GetNotes(ulong memberId) => (storage.Notes.ContainsKey(memberId)) ? storage.Notes[memberId] : new();

    public Note? GetNote(ulong memberId, int noteId) => GetNotes(memberId).Find(x => x.Id == noteId);

    private async Task SearchForExpiredRemindersAsync()
    {
        //Console.WriteLine($"{DateTime.Now}: Scanning for expired note reminders");
        foreach (var x in storage.NoteReminders.Where(x => x.AlertWhen < DateTimeOffset.Now).ToList())
        {
            Note? note = GetNote(x.MemberId, x.NoteId);

            if (note != null)
            {
                var builder = new EmbedBuilder()
                    .WithTitle(note.Name)
                    .WithDescription(note.Contents)
                    .WithColor(Color.Green);

                IUser user = await client.GetUserAsync(x.MemberId);
                IDMChannel channel = await user.CreateDMChannelAsync();
                await channel.SendMessageAsync("The timer is up!", embed: builder.Build());

                if (x.DeleteNoteAfter)
                    await Delete(x.MemberId, x.NoteId);
            }

            await RemoveReminder(x.MemberId, x.NoteId);
        };
    }

    private void SearchForExpiredReminders(object state) => SearchForExpiredRemindersAsync().GetAwaiter().GetResult();
}