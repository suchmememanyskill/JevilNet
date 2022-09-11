﻿using System.Text;
using Newtonsoft.Json;

namespace JevilNet.Services.Model;

public static class Utils
{
    public async static Task<T> Get<T>(string endPoint)
    {
        using var httpClient = new HttpClient();
        // Please don't steal this url. I pay for this and am too lazy to add auth
        var response = await httpClient.GetAsync($"http://152.70.57.126:4624/{endPoint}");
        response.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync())!;
    }
    
    public async static Task Post(string endPoint, object data)
    {
        string strData = JsonConvert.SerializeObject(data);
        using var httpClient = new HttpClient();
        // Please don't steal this url. I pay for this and am too lazy to add auth
        var response = await httpClient.PostAsync($"http://152.70.57.126:4624/{endPoint}", new StringContent(strData, Encoding.Default, "application/json"));
        response.EnsureSuccessStatusCode();
    }
}