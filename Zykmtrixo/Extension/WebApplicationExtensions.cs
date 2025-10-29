using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Zykmtrixo.Web.Server.Model.Request;
using Zykmtrixo.Web.Server.Model.Response;

namespace Zykmtrixo.Extension;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps a POST request for the web application.
    /// </summary>
    /// <param name="this">WebApplication to add the route to.</param>
    /// <param name="pattern">Pattern for the URL to map.</param>
    /// <param name="requestDelegate">Request handler, which takes in a RequestContext and returns a JsonResponse.</param>
    public static void MapPostWithContext(this IEndpointRouteBuilder @this, string pattern, Func<RequestContext, Task<JsonResponse>> requestDelegate)
    {
        @this.MapPost(pattern, async (httpContext) =>
        {
            var requestContext = new RequestContext(httpContext);
            var response = await requestDelegate(requestContext);
            await response.GetResponse().ExecuteAsync(httpContext);
        });
    }
}