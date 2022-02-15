using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using JevilNet.Services;
using JevilNet.Services.Models;

namespace JevilNet.Modules.Interactions;

public class NoteInteractions : InteractionModuleBase<SocketInteractionContext>
{
    public NoteService NoteService { get; set; }
    protected bool forceEphemeral = false;
    public async Task RespondEphemeral(string text = null, Embed embed = null, MessageComponent components = null)
    {
        if (!forceEphemeral && Context.Interaction is SocketMessageComponent)
        {
            SocketMessageComponent interaction = Context.Interaction as SocketMessageComponent;
            await interaction.UpdateAsync(x =>
            {
                x.Content = text;
                x.Embed = embed;
                x.Components = components;
            });
        }
        else await RespondAsync(text, embed: embed, components: components, ephemeral:true);
    }

    // Spawns a modal to add a note
    [ComponentInteraction("note_add_button")]
    public async Task NoteAdd()
    {
        await Context.Interaction.RespondWithModalAsync<NoteModal>("note_add_modal");
    }

    [ComponentInteraction("note_view_button")]
    public async Task NotesDisplay()
    {
        List<Note> currentUsersNotes = NoteService.GetNotes(Context.User.Id);
        if (currentUsersNotes.Count < 1)
        {
            await RespondAsync("You have no notes stored!", ephemeral:true);
            return;
        }
        
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select a note to display")
            .WithCustomId("note_menu")
            .WithMinValues(1)
            .WithMaxValues(1);
        
        currentUsersNotes.ForEach(x => menuBuilder.AddOption(x.Name, x.Id.ToString()));
        
        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

        await RespondAsync("Please select a note to view:", components: builder.Build(), ephemeral:true);
    }

    [ComponentInteraction("note_menu")]
    public async Task NoteDisplay(params string[] selections)
    {
        if (selections.Length != 1)
            return;

        Note? note = NoteService.GetNote(Context.User.Id, int.Parse(selections[0]));

        if (note == null)
            return;

        var builder = new EmbedBuilder()
            .WithTitle(note.Name)
            .WithDescription(note.Contents)
            .WithColor(Color.Green);

        var buttonBuilder = new ComponentBuilder()
            .WithButton("Edit note", "note_edit_button:" + note.Id)
            .WithButton("Share note", "note_share_button:" + note.Id, style: ButtonStyle.Secondary)
            .WithButton("Set reminder on note", "note_reminder_button:" + note.Id, style: ButtonStyle.Secondary)
            .WithButton("Delete note", "note_delete_button:" + note.Id, style: ButtonStyle.Danger);
        
        await RespondEphemeral(embed: builder.Build(), components: buttonBuilder.Build());
    }
    
    // Spawns a modal to edit a note
    [ComponentInteraction("note_edit_button:*")]
    [Discord.Interactions.RequireOwner]
    public async Task NoteEdit(string id)
    {
        Note? note = NoteService.GetNote(Context.User.Id, int.Parse(id));

        if (note == null)
            return;

        var builder = new ModalBuilder()
            .WithTitle("Edit Note")
            .WithCustomId("note_edit_modal:" + id)
            .AddTextInput("Title", "title", maxLength: 32, value: note.Name)
            .AddTextInput("Content", "content", maxLength: 500, style: TextInputStyle.Paragraph, value: note.Contents);

        
        await Context.Interaction.RespondWithModalAsync(builder.Build());
    }
    
    [ComponentInteraction("note_delete_button:*")]
    public async Task NoteDelete(string id)
    {
        Note? note = NoteService.GetNote(Context.User.Id, int.Parse(id));

        if (note == null)
        {
            await RespondEphemeral("The note does not exist (how did you get here?)");
            return;
        }
        
        var builder = new EmbedBuilder()
            .WithTitle(note.Name)
            .WithDescription(note.Contents)
            .WithColor(Color.Red);
        
        await NoteService.Delete(Context.User.Id, int.Parse(id));
        await RespondEphemeral("Deleted note", embed: builder.Build());
    }
    
    [ComponentInteraction("note_share_button:*")]
    public async Task NoteShare(string id)
    {
        Note? note = NoteService.GetNote(Context.User.Id, int.Parse(id));

        if (note == null)
            return;

        var builder = new EmbedBuilder()
            .WithTitle(note.Name)
            .WithDescription(note.Contents)
            .WithColor(Color.Red);

        await RespondAsync($"{Context.User.Username} shared a note with you!", embed: builder.Build());
    }
    
    [ComponentInteraction("note_reminder_button:*")]
    public async Task NoteStartReminder(string id)
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("When?")
            .WithCustomId("reminder_menu:" + id)
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Cancel reminder", "0");
        
        Enumerable.Range(1, 6).Select(x => x * 15).ToList().ForEach(x => menuBuilder.AddOption($"In {x} minutes", x.ToString()));
        Enumerable.Range(0, 4).Select(x => (x + 2) * 60).ToList().ForEach(x => menuBuilder.AddOption($"In {x / 60} hours", (x).ToString()));
        Enumerable.Range(1, 4).Select(x => (x * 6) * 60).ToList().ForEach(x => menuBuilder.AddOption($"In {x / 60} hours", (x).ToString()));
        
        await RespondEphemeral("When would you like to be reminded",
            components: new ComponentBuilder().WithSelectMenu(menuBuilder).Build());
    }

    [ComponentInteraction("reminder_menu:*")]
    public async Task NoreReminderMenu(string id, params string[] selections)
    {
        if (selections.Length != 1)
            return;

        NoteReminder? reminder = await NoteService.AddReminder(Context.User.Id, int.Parse(id), int.Parse(selections[0]));

        if (reminder == null)
            await RespondEphemeral("Successfully removed reminder");
        else
            await RespondEphemeral($"Successfully applied reminder. Reminder will go off in <t:{reminder.AlertWhen.ToUnixTimeSeconds()}:R>");
    }

    public class NoteModal : IModal
    {
        public string Title => "Create Note";
        // Strings with the ModalTextInput attribute will automatically become components.
        [InputLabel("Title")]
        [ModalTextInput("title", placeholder: "My epic note", maxLength: 32)]
        public string NoteTitle { get; set; }

        // Additional paremeters can be specified to further customize the input.
        [InputLabel("Content")]
        [ModalTextInput("content", TextInputStyle.Paragraph, "Markdown is supported", maxLength: 500)]
        public string Content { get; set; }
    }

    [ModalInteraction("note_add_modal")]
    public async Task NoteAddModal(NoteModal modal)
    {
        Note note = await NoteService.Add(Context.User.Id, modal.NoteTitle, modal.Content);
        forceEphemeral = true;
        await NoteDisplay(note.Id.ToString());
    }
    
    [ModalInteraction("note_edit_modal:*")]
    public async Task NoteEditModal(string id, NoteModal modal)
    {
        await NoteService.Edit(Context.User.Id, int.Parse(id), modal.NoteTitle, modal.Content);
        forceEphemeral = true;
        await NoteDisplay(id);
    }
}