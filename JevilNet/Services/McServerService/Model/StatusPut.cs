namespace JevilNet.Services.Model;

public class StatusPut
{
    public bool Status { get; set; }
    public static async Task Post(bool state) => await Utils.Put("Status/state", new StatusPut() { Status = state });
}