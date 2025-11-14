using System.Net.Http;
using System.Threading.Tasks;
using Zykmtrixo.Diagnostic;
using Zykmtrixo.State;
using Zykmtrixo.Web.Client;
using Zykmtrixo.Web.Server.Model.Request;
using Zykmtrixo.Web.Server.Model.Response;

namespace Zykmtrixo.Web.Server.Controller;

public class ShutdownController
{
    /// <summary>
    /// Handles a shutdown request.
    /// </summary>
    /// <returns>Response for the shutdown.</returns>
    public async Task<JsonResponse> HandleShutdownRequest(RequestContext context)
    {
        // Return if the request can't be parsed.
        // While HTTP 400 is more useful, 401 is preferred since it must be parsed before being authorized.
        var request = context.GetRequest(ShutdownRequestJsonContext.Default.ShutdownRequest);
        if (request?.PlaceId == null)
        {
            Logger.Debug("Received invalid request (malformed JSON or missing placeId).");
            return new JsonResponse(new BasicResponse(ResponseStatus.Unauthorized), 401);
        }
        
        // Return if the place id isn't supported.
        var placeId = request.PlaceId.Value;
        var configuration = ConfigurationState.Instance.CurrentConfiguration;
        var placeConfiguration = configuration.Roblox.GetEntryForPlaceId(placeId);
        if (placeConfiguration == null)
        {
            Logger.Debug($"A shutdown request for {placeId} was received but no configuration is stored.");
            return new JsonResponse(new BasicResponse(ResponseStatus.Unauthorized), 401);
        }
        
        // Return if the request isn't authorized.
        if (!context.IsAuthorized(placeConfiguration.ApiKey, placeConfiguration.SecretKey))
        {
            Logger.Debug($"A shutdown request for {placeId} was received but was unauthorized.");
            return new JsonResponse(new BasicResponse(ResponseStatus.Unauthorized), 401);
        }
        
        // Return if the JobId is missing.
        if (request.JobId == null)
        {
            Logger.Debug("Received invalid request (missing jobId).");
            return new JsonResponse(new BasicResponse(ResponseStatus.MalformedRequest), 400);
        }
        
        // Send the shutdown request.
        var client = new RobloxUserClient(placeId);
        try
        {
            Logger.Debug($"Requesting shutdown for place id {placeId} job id {request.JobId}.");
            await client.ShutDownAsync(request.JobId);
            Logger.Info($"Requested shutdown for place id {placeId} job id {request.JobId}.");
            return new JsonResponse(new BasicResponse(ResponseStatus.Success), 200);
        }
        catch (HttpRequestException e)
        {
            Logger.Error($"An error occured shutting down place id {placeId} job id {request.JobId}: {e.Message}");
            return new JsonResponse(new BasicResponse(ResponseStatus.ServerError), 500);
        }
    }
}