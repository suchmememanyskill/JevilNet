using Discord;
using Discord.Interactions;
using JevilNet.Modules.Interactions;
using JevilNet.Services;
using JevilNet.Services.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Modules.SlashCommands;

[Group("note", "Note utilities")]
public class NoteSlashCommands : NoteInteractions
{
    [SlashCommand("add", "Opens a form where a new note can be added")]
    public Task CreateNote() => Context.Interaction.RespondWithModalAsync<NoteModal>("note_add_modal");

    [SlashCommand("view", "View a note")]
    public Task ViewNote([Autocomplete(typeof(ViewNoteAutocompleteHandler))] int type) => NoteDisplay(type.ToString());

    public class ViewNoteAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            NoteService service = services.GetRequiredService<NoteService>();
            List<Note> notes = service.GetNotes(context.User.Id);

            return AutocompletionResult.FromSuccess(notes.Select(x => new AutocompleteResult(x.Name, x.Id)).ToList());
        }
    }
}