using System.Threading.Tasks;
using Zykmtrixo.Diagnostic;
using Zykmtrixo.State;
using Zykmtrixo.Web.Server;

namespace Zykmtrixo;

public class Program
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">Arguments from the command line.</param>
    public static async Task Main(string[] args)
    {
        // Set the minimum log level.
        var configurationState = ConfigurationState.Instance;
        Logger.SetMinimumLogLevel(configurationState.CurrentConfiguration.Logging.MinimumLogLevel);
        configurationState.ConfigurationChanged += (newConfiguration) =>
        {
            Logger.SetMinimumLogLevel(newConfiguration.Logging.MinimumLogLevel);
        };
        
        // Run the server.
        await new WebServer().StartAsync();
        await Logger.WaitForCompletionAsync();
    }
}