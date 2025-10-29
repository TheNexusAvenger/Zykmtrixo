using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;
using Zykmtrixo.Diagnostic;

namespace Zykmtrixo.Web.Server.Model.Request;

public class RequestContext
{
    /// <summary>
    /// Authorization header from the request.
    /// </summary>
    public readonly string? Authorization;
    
    /// <summary>
    /// Contents of the request body.
    /// </summary>
    public readonly string RequestBody;

    /// <summary>
    /// Creates a request context.
    /// </summary>
    /// <param name="authorization">Authorization header from the request.</param>
    /// <param name="requestBody">Contents of the request body.</param>
    public RequestContext(string? authorization, string requestBody)
    {
        this.Authorization = authorization;
        this.RequestBody = requestBody;
    }

    /// <summary>
    /// Creates a request context.
    /// </summary>
    /// <param name="httpContext">HttpContext to read the request from.</param>
    public RequestContext(HttpContext httpContext)
    {
        this.Authorization = httpContext.Request.Headers.Authorization.FirstOrDefault();
        this.RequestBody = new StreamReader(httpContext.Request.Body).ReadToEndAsync().Result;
    }

    /// <summary>
    /// Returns if the request is authorized.
    /// </summary>
    /// <param name="apiKey">API key to allow.</param>
    /// <param name="secretKey">Secret key to allow.</param>
    /// <returns>If the response was authorized.</returns>
    public bool IsAuthorized(string? apiKey, string? secretKey)
    {
        // Split the authorization parts.
        var authorizationParts = this.Authorization!.Split(' ', 2);
        if (authorizationParts.Length != 2)
        {
            Logger.Debug("Authorization header did not start with ApiKey, Bearer, or Signature.");
            return false;
        }
        
        // Handle the cases.
        var authorizationType = authorizationParts[0].ToLower();
        if ((authorizationType == "apikey" || authorizationType == "bearer") && apiKey != null)
        {
            var matches = authorizationParts[1] == apiKey;
            if (!matches)
            {
                Logger.Debug("Authorization API key did not match..");
            }
            return matches;
        }
        if (authorizationType == "signature" && secretKey != null)
        {
            using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var newSignature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(this.RequestBody)));
            var matches = newSignature == authorizationParts[1];
            if (!matches)
            {
                Logger.Debug("Authorization signature did not match.");
            }
            return matches;
        }
        Logger.Debug("Authorization header did not start with ApiKey, Bearer, or Signature.");
        return false;
    }
    
    /// <summary>
    /// Returns the request object from the body.
    /// </summary>
    /// <param name="jsonTypeInfo">JSON type information of the request.</param>
    /// <typeparam name="T">Type of the request to parse.</typeparam>
    /// <returns>Request body, if it could be parsed.</returns>
    public T? GetRequest<T>(JsonTypeInfo<T> jsonTypeInfo)
    {
        try
        {
            return JsonSerializer.Deserialize(this.RequestBody, jsonTypeInfo);
        }
        catch (Exception e)
        {
            // JSON is malformed in this case.
            Logger.Trace($"Error processing JSON request: {e}");
            return default;
        }
    }
}