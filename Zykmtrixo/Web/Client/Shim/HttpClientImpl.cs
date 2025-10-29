using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Zykmtrixo.Web.Client.Shim;

public class HttpClientImpl : IHttpClient
{
    /// <summary>
    /// HttpClient to send requests with.
    /// </summary>
    private readonly HttpClient _httpClient = new HttpClient();
    
    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <returns>Response for the request.</returns>
    public async Task<HttpStringResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var response = await this._httpClient.SendAsync(request);
        var headers = new Dictionary<string, string>();
        foreach (var header in response.Headers)
        {
            var headerValue = header.Value.FirstOrDefault();
            if (headerValue == null) continue;
            headers.Add(header.Key.ToLower(), headerValue);
        }
        return new HttpStringResponseMessage()
        {
            StatusCode = response.StatusCode,
            Headers = headers,
            Content = await response.Content.ReadAsStringAsync(),
        };
    }
}