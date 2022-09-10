namespace JevilNet.Services.Model;

public class MapsPost
{
    public string MapName { get; set; }
    public static async Task Post(string mapName) => await Utils.Post("Maps", new MapsPost() { MapName = mapName });
}