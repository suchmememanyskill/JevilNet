namespace JevilNet.Services.Model;

public class VersionsPut
{
    public string Version { get; set; }

    public static async Task Post(string version) =>
        await Utils.Put("Versions", new VersionsPut() { Version = version });
}