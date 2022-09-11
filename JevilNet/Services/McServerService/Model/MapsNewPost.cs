namespace JevilNet.Services.Model;

public class MapsNewPost
{
    public string MinecraftVersion { get; set; }
    public string Name { get; set; }

    public static async Task Post(string name, string minecraftVersion) => await Utils.Post("Maps/new",
        new MapsNewPost() { MinecraftVersion = minecraftVersion, Name = name });
}