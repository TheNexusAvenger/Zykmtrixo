using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Zykmtrixo.Diagnostic;
using Zykmtrixo.State;
using Zykmtrixo.Web.Client;
using Zykmtrixo.Web.Server.Model.Response;

namespace Zykmtrixo.Web.Server.Controller;

public class HealthController
{
    /// <summary>
    /// Time that the health check is considered state.
    /// </summary>
    public static readonly TimeSpan HealthcheckStaleTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Time of the last health check.
    /// </summary>
    private DateTime LastHealthCheckTime { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Cached response for the health controller.
    /// </summary>
    private BasicResponse LastResponse { get; set; } = new BasicResponse(ResponseStatus.Success);
    
    /// <summary>
    /// Semaphore for updating health checks.
    /// </summary>
    private readonly SemaphoreSlim _healthCheckLock = new SemaphoreSlim(1);

    /// <summary>
    /// Handles a health check request.
    /// </summary>
    /// <returns>Response for the health check.</returns>
    public async Task<JsonResponse> HandleHealthCheck()
    {
        try
        {
            // Return a cached response.
            await _healthCheckLock.WaitAsync();
            if (DateTime.Now - LastHealthCheckTime < HealthcheckStaleTime)
            {
                Logger.Debug($"Returning previous {this.LastResponse.Status} health check.");
                return new JsonResponse(this.LastResponse, this.LastResponse.Status == ResponseStatus.Success ? 200 : 503);
            }
            
            // Determine a new health check.
            // This uses an invalid job id, which will return a successful response.
            Logger.Debug("Performing health check.");
            var success = true;
            var configuration = ConfigurationState.Instance.CurrentConfiguration;
            foreach (var placeConfiguration in configuration.Roblox.Places)
            {
                foreach (var placeId in placeConfiguration.PlaceIds)
                {
                    try
                    {
                        Logger.Debug($"Sending health check to place id {placeId}.");
                        await new RobloxUserClient(placeId).ShutDownAsync("00000000-0000-0000-0000-000000000000");
                        Logger.Debug($"Health check to place id {placeId} success.");
                    }
                    catch (HttpRequestException e)
                    {
                        Logger.Error($"Error sending health check to place id {placeId}: {e.Message}");
                        success = false;
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error sending health check to place id {placeId}: e");
                        success = false;
                    }
                }
            }
            
            // Store and return the response.
            this.LastResponse = new BasicResponse(success ? ResponseStatus.Success : ResponseStatus.ServerError);
            this.LastHealthCheckTime = DateTime.Now;
            Logger.Debug($"Returning {this.LastResponse.Status} health check.");
            return new JsonResponse(this.LastResponse, this.LastResponse.Status == ResponseStatus.Success ? 200 : 503);
        }
        finally
        {
            _healthCheckLock.Release();
        }
    }
}