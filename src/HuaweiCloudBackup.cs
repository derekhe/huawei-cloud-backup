using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class GalleryStatInfoQuery
{
    int needRefresh = 0;
    string traceId = "04113_02_1595943080_87857589";
}

class HuaweiCloudBackup
{
    public async Task run()
    {
        var cookies = new EdgeCookieReader().GetEdgeCookie(".cloud.huawei.com");
        var CSRFToken = cookies["CSRFToken"];
        var handler = new HttpClientHandler() { UseCookies = true };

        HttpClient client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("CSRFToken", CSRFToken.Value);

        var json = JsonConvert.SerializeObject(new Dictionary<string, string>
        {
            ["needRefresh"] = "0",
            ["traceId"] = "04113_02_1595943080_87857589"
        });

        var builder = new UriBuilder(new Uri("https://cloud.huawei.com/album/galleryStatInfo"));

        var cookieContainer = new CookieContainer();
        cookieContainer.Add(cookies);
        handler.CookieContainer = cookieContainer;

        var request = new HttpRequestMessage(HttpMethod.Post, builder.Uri);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);

        string result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine("Result is " + result);
    }
}