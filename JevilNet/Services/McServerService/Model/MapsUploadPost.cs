using System.Web;

namespace JevilNet.Services.Model;

public class MapsUploadPost
{
    public static async Task Post(string mapName, string mcVersion, Stream file, bool readOnly = false)
    {
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["suggested_mc_version"] = mcVersion;
        parameters["read_only"] = readOnly.ToString();
        await Utils.PostFile($"Maps/{mapName}?{parameters}", file);
    }
}