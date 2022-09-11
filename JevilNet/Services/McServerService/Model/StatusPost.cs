namespace JevilNet.Services.Model;

public class StatusPost
{
    public bool Status { get; set; }
    public static async Task Post(bool state) => await Utils.Post("Status/state", new StatusPost() { Status = state });
}