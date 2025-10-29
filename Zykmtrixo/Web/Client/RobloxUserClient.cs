using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Zykmtrixo.State;
using Zykmtrixo.Web.Client.Model.Request;
using Zykmtrixo.Web.Client.Model.Response;
using Zykmtrixo.Web.Client.Shim;

namespace Zykmtrixo.Web.Client;

public class RobloxUserClient
{
    /// <summary>
    /// Override HttpClient implementation to use.
    /// Intended to be set for unit testing.
    /// </summary>
    public static IHttpClient? HttpClient { get; set; }
    
    /// <summary>
    /// Cache of CSRF tokens to use for requests.
    /// </summary>
    private static readonly Dictionary<int, string> CsrfTokens = new Dictionary<int, string>();
    
    /// <summary>
    /// Lock for handling the CSRF tokens.
    /// </summary>
    private static readonly ReaderWriterLockSlim CsrfTokenLock = new ReaderWriterLockSlim();

    /// <summary>
    /// Place id for the client.
    /// </summary>
    public readonly long PlaceId;
    
    /// <summary>
    /// HTTP client to send requests.
    /// </summary>
    private readonly IHttpClient _httpClient;

    /// <summary>
    /// Session cookie for the client.
    /// </summary>
    private readonly string _sessionCookie;

    /// <summary>
    /// Creates a Roblox user client.
    /// </summary>
    public RobloxUserClient(long placeId)
    {
        var configuration = ConfigurationState.Instance.CurrentConfiguration;
        this.PlaceId = placeId;
        this._httpClient = HttpClient ?? new HttpClientImpl();
        this._sessionCookie = configuration.Roblox.GetEntryForPlaceId(placeId)!.SessionCookie;
    }
    
    /// <summary>
    /// Returns the cached CSRF token for a session cookie.
    /// </summary>
    /// <returns>The cached CSRF token, if one exists.</returns>
    private string? GetCachedCsrfToken()
    {
        CsrfTokenLock.EnterReadLock();
        try
        {
            return CsrfTokens.GetValueOrDefault(this._sessionCookie.GetHashCode());
        }
        finally
        {
            CsrfTokenLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Stores a CSRF token for a session cookie.
    /// </summary>
    /// <param name="token">Session cookie to store.</param>
    private void StoreCsrfToken(string token)
    {
        CsrfTokenLock.EnterWriteLock();
        CsrfTokens[this._sessionCookie.GetHashCode()] = token;
        CsrfTokenLock.ExitWriteLock();
    }
    
    /// <summary>
    /// Performs a request to Roblox.
    /// </summary>
    /// <param name="httpMethod">HTTP method to send.</param>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <param name="content">Content of the request.</param>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    private async Task<TResponse> InternalRequestAsync<TResponse>(HttpMethod httpMethod, string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo, HttpContent? content = null)
    {
        // Perform the request.
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Headers =
            {
                {"Cookie", this._sessionCookie},
                {"X-Csrf-Token", this.GetCachedCsrfToken()},
            },
            Method = httpMethod,
        };
        if (content != null)
        {
            request.Content = content;
        }
        var response = await this._httpClient.SendAsync(request);
        
        // Store the X-Csrf-Token.
        if (response.Headers.TryGetValue("x-csrf-token", out var newCrsfToken))
        {
            this.StoreCsrfToken(newCrsfToken);
        }
        
        // Throw an exception if Roblox returned an error.
        if ((int) response.StatusCode >= 300)
        {
            throw new HttpRequestException(HttpRequestError.InvalidResponse,
                $"Error sending to {url}: HTTP {(int) response.StatusCode} ({response.StatusCode}) - {response.Content}",
                statusCode: response.StatusCode);
        }
        
        // Parse and return the response.
        return JsonSerializer.Deserialize<TResponse>(response.Content, jsonResponseTypeInfo)!;
    }

    /// <summary>
    /// Performs a request to Roblox.
    /// </summary>
    /// <param name="httpMethod">HTTP method to send.</param>
    /// <param name="url">URL to request.</param>
    /// <param name="jsonResponseTypeInfo">JSON type information to deserialize the response.</param>
    /// <param name="request">Content of the request.</param>
    /// <param name="jsonRequestTypeInfo">JSON type information to deserialize the request.</param>
    /// <typeparam name="TRequest">Type of the request.</typeparam>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <returns>JSON response for the request.</returns>
    public async Task<TResponse> RequestAsync<TRequest, TResponse>(HttpMethod httpMethod, string url, JsonTypeInfo<TResponse> jsonResponseTypeInfo, TRequest request, JsonTypeInfo<TRequest> jsonRequestTypeInfo)
    {
        try
        {
            // Send the initial request.
            return await this.InternalRequestAsync(httpMethod, url, jsonResponseTypeInfo, JsonContent.Create(request, jsonRequestTypeInfo));
        }
        catch (HttpRequestException exception)
        {
            // Resend the request with the stored CSRF token if it was HTTP 403, otherwise throw.
            if (exception.StatusCode == HttpStatusCode.Forbidden)
            {
                return await this.InternalRequestAsync(httpMethod, url, jsonResponseTypeInfo, JsonContent.Create(request, jsonRequestTypeInfo));
            }
            throw;
        }
    }

    /// <summary>
    /// Sends a request to shut down a server.
    /// </summary>
    /// <param name="jobId">Job id of the server.</param>
    /// <returns>Response for the shutdown if it was successful.</returns>
    public async Task<RobloxServerShutdownResponse> ShutDownAsync(string jobId)
    {
        return await this.RequestAsync(HttpMethod.Post,
            "https://apis.roblox.com/matchmaking-api/v1/game-instances/shutdown",
            RobloxServerShutdownResponseJsonContext.Default.RobloxServerShutdownResponse,
            new RobloxServerShutdownRequest()
            {
                PlaceId = this.PlaceId,
                GameId = jobId,
            }, RobloxServerShutdownRequestJsonContext.Default.RobloxServerShutdownRequest);
    }
}