namespace JevilNet.Services.Model;

public class StatusGet
{
    public MapsGet? Map { get; set; }
    public VersionsGet? Version { get; set; }
    public string TextStatus { get; set; } = "";
    public List<string> OnlinePlayers { get; set; } = new();

    public bool IsOffline => TextStatus is "Dead" or "Stopped";

    public async static Task<StatusGet> Get() => await Utils.Get<StatusGet>("Status");
}