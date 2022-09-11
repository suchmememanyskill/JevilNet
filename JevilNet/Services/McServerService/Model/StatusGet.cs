namespace JevilNet.Services.Model;

public class StatusGet
{
    public string MapName { get; set; } = "";
    public string ServerVersion { get; set; } = "";
    public string TextStatus { get; set; } = "";
    public List<string> OnlinePlayers { get; set; } = new();

    public async static Task<StatusGet> Get() => await Utils.Get<StatusGet>("Status");
}