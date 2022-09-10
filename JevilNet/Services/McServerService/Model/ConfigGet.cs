namespace JevilNet.Services.Model;

public class ConfigGet
{
    public string MapName { get; set; }
    public string ServerVersion { get; set; }
    public string TextStatus { get; set; }

    public async static Task<ConfigGet> Get() => await Utils.Get<ConfigGet>("Config");
}