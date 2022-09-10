namespace JevilNet.Services.Model;

public class MapsGet
{
    public string Name { get; set; }
    public string MinecraftVersion { get; set; }

    public static async Task<List<MapsGet>> Get() => await Utils.Get<List<MapsGet>>("Maps");
}
