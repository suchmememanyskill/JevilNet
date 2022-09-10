namespace JevilNet.Services.Model;

public class VersionsGet
{
    public string Version { get; set; }
    public string JavaVersion { get; set; }
    public bool UsesMaps { get; set; }
    public static async Task<List<VersionsGet>> Get() => await Utils.Get<List<VersionsGet>>("Versions");
}