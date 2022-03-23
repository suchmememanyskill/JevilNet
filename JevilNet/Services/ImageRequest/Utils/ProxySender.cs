using System.Net;

namespace JevilNet.Services.ImageRequest.Utils;

public class ProxySender
{
    private string proxy;
    private NetworkCredential? credentials;
    
    public ProxySender(string proxy, NetworkCredential credentials = null)
    {
        this.proxy = proxy;
        this.credentials = credentials;
    }

    public static ProxySender GetDefault() => new("socks5://82.196.7.200:2434", new("vpn", "unlimited"));

    public async Task<string> Get(Uri uri, NetworkCredential? credential = null)
    {
        Console.WriteLine($"Requesting {uri}");
        
        var handler = new HttpClientHandler()
        {
            Proxy = new WebProxy(new Uri(proxy)),
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        
        if (credentials != null)
            handler.Proxy.Credentials = credentials;

        if (credential != null)
            handler.Credentials = credential;

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", "Jevil/Bot");
        
        client.Timeout = TimeSpan.FromSeconds(10);
        for (int i = 0; i < 3; i++)
        {
            try
            {
                string response = await client.GetStringAsync(uri);
                if (response.Contains("API limited"))
                    throw new Exception($"Api limited: {response}");

                await Task.Delay(1000); // Don't spam the servers
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Request failed! {e.Message}");
            }
        }

        throw new Exception($"Request failed after 3 retries");
    }
}