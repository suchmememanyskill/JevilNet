using Discord;
using Discord.Commands;
using JevilNet.Attributes;
using JevilNet.Services;

namespace JevilNet.Modules.TextCommands;

[Discord.Commands.Group("note")]
[Alias("notes")]
[Summary("Text commands for note module")]
public class NoteTextCommands : ModuleBase<SocketCommandContext>
{
    public NoteService NoteService { get; set; }
    
    [Command]
    [Summary("Initiates the note interaction menu")]
    public async Task InitiateNoteMenu()
    {
        var builder = new ComponentBuilder()
            .WithButton("Add note", "note_add_button")
            .WithButton("View notes", "note_view_button");

        await ReplyAsync("Welcome to notes!", components: builder.Build());
    }

    [TextCommand("add")]
    [Summary("Adds a note via a text command")]
    public async Task AddNoteWithText([Summary("Hello")] string name, params string[] content)
    {
        if (content.Length < 1)
            await ReplyAsync("Missing content");

        string combinedContent = string.Join(" ", content);

        await NoteService.Add(Context.User.Id, name, combinedContent);
        await Context.Message.AddReactionAsync(Emoji.Parse(":+1:"));
    }
}