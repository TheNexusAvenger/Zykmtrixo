using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Zykmtrixo.Diagnostic;

namespace Zykmtrixo.State;

public class ConfigurationState
{
    /// <summary>
    /// Static instance of a configuration state.
    /// </summary>
    public static readonly ConfigurationState Instance = new ConfigurationState();
    
    /// <summary>
    /// Event for the configuration changing.
    /// </summary>
    public event Action<Configuration>? ConfigurationChanged;

    /// <summary>
    /// Loaded configuration instance of the state.
    /// </summary>
    public Configuration CurrentConfiguration { get; private set; } = null!;

    /// <summary>
    /// Last configuration as JSON.
    /// </summary>
    private string? _lastConfiguration = null;
    
    /// <summary>
    /// Returns the configuration file path.
    /// </summary>
    /// <returns>Path of the configuration file.</returns>
    public static string GetConfigurationPath()
    {
        return Environment.GetEnvironmentVariable("CONFIGURATION_FILE_LOCATION") ?? "configuration.json";
    }
    
    /// <summary>
    /// Creates a configuration state.
    /// </summary>
    private ConfigurationState()
    {
        // Load the initial configuration.
        this.ReloadAsync().Wait();
        
        // Set up file change notifications.
        var configurationPath = GetConfigurationPath();
        var fileSystemWatcher = new FileSystemWatcher(Directory.GetParent(configurationPath)!.FullName);
        fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
        fileSystemWatcher.Changed += async (_, _) => await this.TryReloadAsync();
        fileSystemWatcher.EnableRaisingEvents = true;
        
        // Occasionally reload the file in a loop.
        // File change notifications don't seem to work in Docker with volumes.
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(10000);
                await this.TryReloadAsync();
            }
        });
    }
        
    /// <summary>
    /// Reloads the configuration.
    /// </summary>
    public async Task ReloadAsync()
    {
        // Prepare the configuration if it doesn't exist.
        var path = GetConfigurationPath();
        if (!File.Exists(path))
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new Configuration(), ConfigurationJsonContext.Default.Configuration));
        }
        
        // Read the configuration.
        Logger.Trace("Attempting to read new configuration.");
        var configurationContents = await File.ReadAllTextAsync(path);
        this.CurrentConfiguration = JsonSerializer.Deserialize<Configuration>(configurationContents, ConfigurationJsonContext.Default.Configuration)!;
        Logger.Trace("Read new configuration.");
        
        // Invoke the changed event if the contents changed.
        if (this._lastConfiguration != null && this._lastConfiguration != configurationContents)
        {
            Logger.Info("Configuration updated.");
            ConfigurationChanged?.Invoke(this.CurrentConfiguration);
        }
        this._lastConfiguration = configurationContents;
    }

    /// <summary>
    /// Tries to reload the configuration.
    /// No exception is thrown if it fails.
    /// </summary>
    public async Task TryReloadAsync()
    {
        try
        {
            await this.ReloadAsync();
        }
        catch (Exception e)
        {
            Logger.Debug($"An error occured trying to update the configuration. This might be due to a text editor writing the file.\n{e}");
        }
    }
}