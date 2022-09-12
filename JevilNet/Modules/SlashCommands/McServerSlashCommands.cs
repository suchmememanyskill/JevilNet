using System.Web;
using Discord;
using Discord.Interactions;
using JevilNet.Modules.Base;
using JevilNet.Services;
using JevilNet.Services.Model;
using JevilNet.Services.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JevilNet.Modules.SlashCommands;

[Group("mc", "Control meme's publicly hosted minecraft server")]
public class McServerSlashCommands : SlashCommandBase
{
    public McServerService McServerService { get; set; }
    private IBaseInterface me => this;

    [SlashCommand("status", "Get the current status of the minecraft server")]
    public async Task Status(bool sendInChannel = false)
    {
        var config = await McServerService.GetStatus();
        string mapName = (config.Map == null) 
            ? "No map has been selected" 
            : (config.Version?.UsesMaps ?? true) 
                ? $"Map{(((config.Map?.ReadOnly ?? false) && (config.Version?.UsesMaps ?? true)) ? " (read-only)" : "")}: {config.Map?.Name ?? "(???)"}" 
                : "This minecraft version does not support custom maps";
        string serverVersion = $"Minecraft Version: {config.Version?.Version ?? "(???)"}";
        string playersOnline = (config.OnlinePlayers.Count <= 0)
            ? "There are no players online"
            : $"There are {config.OnlinePlayers.Count} player(s) online: {string.Join(", ", config.OnlinePlayers)}";
        string message = "";

        switch (config.TextStatus)
        {
            case "Stopped":
                message = $"Server is stopped";
                break;
            case "Initialising":
                message = "Server is being created";
                break;
            case "Started":
                message = "Server has been started, waiting until ready";
                break;
            case "Ready":
                message = $"Server is ready. Connect at ip `152.70.57.126`";
                break;
            case "Stopping":
                message = "Server is stopping";
                break;
            case "Dead":
                message = $"Server has crashed!";
                break;
        }

        message += $"\n\n{mapName}\n{serverVersion}\n{playersOnline}";

        if (config.Map?.HasResourcePack ?? false)
            message += $"\n[This map has a resource pack available](http://152.70.57.126:4624/Maps/resources/{HttpUtility.UrlEncode(config.Map.Name)})";

        EmbedBuilder builder = new();
        builder.WithTitle("JevilServer");
        if (config.IsOffline)
            builder.WithColor(255, 0, 0);
        else
            builder.WithColor(0, 255, 0);
        builder.WithDescription(message);

        if (sendInChannel)
            await me.Respond(embed: builder.Build());
        else
            await me.RespondEphermeral(embed: builder.Build());
    }

    [SlashCommand("reload", "Reload maps and versions")]
    public async Task Reload()
    {
        await McServerService.Reload();
        await me.RespondEphermeral("Reload complete!");
    }

    [SlashCommand("custom_version", "Set the server version")]
    public async Task SetVersion([Autocomplete(typeof(McCustomVersionSuggestions))] string version)
    {
        VersionsGet set = await McServerService.SetVersion(version);

        string append = "";
        if (!set.UsesMaps)
            append = "Note this minecraft version does not support custom maps";
        
        await me.RespondEphermeral($"Set minecraft version to {version}\n{append}");
    }

    [SlashCommand("map", "Set the server map")]
    public async Task SetMap([Autocomplete(typeof(McMapSuggestions))] string map)
    {
        MapsGet set = await McServerService.SetMap(map);

        string append = "";
        if (set.MinecraftVersion != "unk")
            append = $"The minecraft version has also been set to {set.MinecraftVersion}";

        if (set.ReadOnly)
            append += "\nMap is ready only. Any changes made to the map will be lost after a reboot";
            
        await me.RespondEphermeral($"Set minecraft map to {map}\n{append.Trim()}");
    }

    [SlashCommand("on", "Turn the minecraft server on")]
    public async Task SetStateOn([Autocomplete(typeof(McMapSuggestions))] string? map = null)
    {
        string message = "Turned server on";
        if (map != null)
        {
            MapsGet set = await McServerService.SetMap(map);
            
            message += $"\n\nSet minecraft map to {map}";
            
            if (set.MinecraftVersion != "unk")
                message += $"\nThe minecraft version has also been set to {set.MinecraftVersion}";

            if (set.ReadOnly)
                message += "\nMap is ready only. Any changes made to the map will be lost after a reboot";
        }
        
        await McServerService.SetState(true);
        await me.RespondEphermeral(message.Trim());
    }
    
    [SlashCommand("off", "Turn the minecraft server off")]
    public async Task SetStateOff()
    {
        await McServerService.SetState(false);
        await me.RespondEphermeral($"Turned server off");
    }

    [SlashCommand("new", "Create a new map. Think before you type, there's no way to delete this")]
    public async Task CreateNewMap(string name, [Autocomplete(typeof(McVersionSuggestions))] string version)
    {
        await McServerService.CreateMap(name, version);
        await me.RespondEphermeral($"Created new map");
    }

    [SlashCommand("upload", "Upload a map to the server. Needs to be a .zip file containing a 'world' folder")]
    public async Task UploadMap(string name, [Autocomplete(typeof(McVersionSuggestions))] string version,
        IAttachment attachment, bool readOnly = false)
    {
        await DeferAsync(true);
        try
        {
            await McServerService.UploadMap(name, version, attachment.Url, readOnly);
            await FollowupAsync("Upload complete!", ephemeral: true);
        }
        catch (Exception e)
        {
            await FollowupAsync(e.Message, ephemeral: true);
        }
    }

    [SlashCommand("map_version", "Change a map's version. Be careful with downgrading")]
    public async Task ChangeMapVersion([Autocomplete(typeof(McMapSuggestions))] string name, [Autocomplete(typeof(McVersionSuggestions))] string version)
    {
        var res = await McServerService.ChangeMapVersion(name, version);
        await me.RespondEphermeral($"Changed Map {res.Map.Name} from Version {res.OldVersion.Version} to Version {res.NewVersion.Version}");
    }

    [SlashCommand("delete", "Owner only. Deletes a map from the server")]
    public async Task DeleteMap([Autocomplete(typeof(McMapSuggestions))] string name)
    {
        await McServerService.DeleteMap(name);
        await me.RespondEphermeral("Deleted map");
    }

    public class McMapSuggestions : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        { 
            McServerService service = services.GetRequiredService<McServerService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();
            
            return AutocompletionResult.FromSuccess(service.Maps.Where(x => x.Name.ToLower().Contains(search)).Select(x => new AutocompleteResult(x.Name, x.Name)).ToList());
        }
    }
    
    
    public class McVersionSuggestions : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            McServerService service = services.GetRequiredService<McServerService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();
            
            return AutocompletionResult.FromSuccess(service.Versions.Where(x => x.UsesMaps).Where(x => x.Version.ToLower().Contains(search)).Select(x => new AutocompleteResult(x.Version, x.Version)).ToList());
        }
    }
    
    public class McCustomVersionSuggestions : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            McServerService service = services.GetRequiredService<McServerService>();
            string search = (string)autocompleteInteraction.Data.Current.Value;
            search = search.ToLower();
            
            return AutocompletionResult.FromSuccess(service.Versions.Where(x => !x.UsesMaps).Where(x => x.Version.ToLower().Contains(search)).Select(x => new AutocompleteResult(x.Version, x.Version)).ToList());
        }
    }
}