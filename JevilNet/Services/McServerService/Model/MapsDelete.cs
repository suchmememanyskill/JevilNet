namespace JevilNet.Services.Model;

public class MapsDelete
{
    public static async Task Delete(string map_name) => await Utils.Delete($"Maps/{map_name}");
}