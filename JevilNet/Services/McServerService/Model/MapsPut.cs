namespace JevilNet.Services.Model;

public class MapsPut
{
    public string MapName { get; set; }
    public static async Task Post(string mapName) => await Utils.Put("Maps", new MapsPut() { MapName = mapName });
}