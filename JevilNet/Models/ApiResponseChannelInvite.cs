using Newtonsoft.Json;

namespace JevilNet.Models;

public class ApiResponseChannelInvite
{
    [JsonProperty("code")] public string Code { get; set; }
    [JsonProperty("target_application")] public ApiResponseChannelInviteTargetApplication App { get; set; }
    [JsonProperty("expires_at")] public DateTimeOffset ExpiresAt { get; set; }
}

public class ApiResponseChannelInviteTargetApplication
{
    [JsonProperty("icon")] public string Icon { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
}