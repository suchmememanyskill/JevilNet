using Discord.Interactions;
using JevilNet.Services;
using JevilNet.Services.Models;

namespace JevilNet.Modules.SlashCommands;

public class RemindMeSlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    public NoteService NoteService { get; set; }
    
    [SlashCommand("remindme", "I'll remind you after a set amount of time")]
    public async Task RemindMe(TimeSpan time, string text, bool keep = false)
    {
        if (time.TotalMinutes < 1)
        {
            await ReplyAsync("Invalid input");
            return;
        }

        Note newNote = await NoteService.Add(Context.User.Id, $"Reminder in {time.ToString()}", text);
        NoteReminder? reminder = await NoteService.AddReminder(Context.User.Id, newNote.Id, Convert.ToInt32(time.TotalMinutes), !keep);
        await ReplyAsync($"Scheduled reminder <t:{reminder!.AlertWhen.ToUnixTimeSeconds()}:R>");
    }
}