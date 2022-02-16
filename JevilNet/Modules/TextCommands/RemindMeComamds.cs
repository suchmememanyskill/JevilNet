using Discord.Commands;
using JevilNet.Services;
using JevilNet.Services.Models;

namespace JevilNet.Modules.TextCommands;

[Group("remindme")]
[Summary("Reminder addons to the note module")]
public class RemindMeComamds : ModuleBase<SocketCommandContext>
{
    public NoteService NoteService { get; set; }
    
    [Command]
    [Summary("Reminds you after a set amount of time")]
    public Task RemindMeDelete(TimeSpan time, [Remainder] string text) => RemindMe(false, time, text);

    [Command("keep")]
    [Summary("Reminds you after a set amount of time. Keeps the note after")]
    public Task RemindMeKeep(TimeSpan time, [Remainder] string text) => RemindMe(true, time, text);
    
    private async Task RemindMe(bool keep, TimeSpan time, string text)
    {
        if (time.TotalMinutes < 1)
        {
            await ReplyAsync("Invalid input");
            return;
        }

        Note newNote = await NoteService.Add(Context.User.Id, $"Reminder in {time.ToString()}", text);
        NoteReminder reminder = await NoteService.AddReminder(Context.User.Id, newNote.Id, Convert.ToInt32(time.TotalMinutes), !keep);
        await ReplyAsync($"Scheduled reminder <t:{reminder!.AlertWhen.ToUnixTimeSeconds()}:R>");
    }
}