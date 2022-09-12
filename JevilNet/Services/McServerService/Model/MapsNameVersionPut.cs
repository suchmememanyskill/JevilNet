namespace JevilNet.Services.Model;

public class MapsNameVersionPut
{
    public string Version { get; set; }

    public static async Task Put(string map_name, string version) => await Utils.Put($"Maps/{map_name}/version",
        new MapsNameVersionPut() { Version = version });
}