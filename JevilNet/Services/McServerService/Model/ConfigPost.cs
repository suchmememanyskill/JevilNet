namespace JevilNet.Services.Model;

public class ConfigPost
{
    public bool Status { get; set; }
    public static async Task Post(bool state) => await Utils.Post("Config/state", new ConfigPost() { Status = state });
}