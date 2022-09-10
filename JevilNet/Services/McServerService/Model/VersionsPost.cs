namespace JevilNet.Services.Model;

public class VersionsPost
{
    public string Version { get; set; }

    public static async Task Post(string version) =>
        await Utils.Post("Versions", new VersionsPost() { Version = version });
}