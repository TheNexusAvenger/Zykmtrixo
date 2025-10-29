using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Zykmtrixo.Diagnostic;
using Zykmtrixo.Extension;
using Zykmtrixo.State;
using Zykmtrixo.Web.Server.Controller;

namespace Zykmtrixo.Web.Server;

public class WebServer
{
    /// <summary>
    /// Starts the web server.
    /// </summary>
    public async Task StartAsync()
    {
        // Create the app builder with custom logging.
        var configuration = ConfigurationState.Instance.CurrentConfiguration;
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        if (configuration.Logging.AspNetLoggingEnabled)
        {
            builder.Logging.AddProvider(Logger.NexusLogger);
        }
        builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
        
        // Set up custom exception handling.
        var app = builder.Build();
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature != null)
                {
                    Logger.Error($"An exception occurred processing {context.Request.Method} {context.Request.Path}\n{exceptionHandlerPathFeature.Error}");
                }
                return Task.CompletedTask;
            });
        });
        
        // Build the API.
        var healthController = new HealthController();
        app.MapGet("/health", async (httpContext) =>
        {
            await (await healthController.HandleHealthCheck()).GetResponse().ExecuteAsync(httpContext);
        });

        var shutdownController = new ShutdownController();
        app.MapPostWithContext("/shutdown", async (context) =>
            await shutdownController.HandleShutdownRequest(context));
        
        // Run the server.
        var port = configuration.Server.Port;
        Logger.Info($"Serving on port {port}.");
        await app.RunAsync($"http://{configuration.Server.Host}:{port}");
    }
}