using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Zykmtrixo.State;

public class LoggingConfiguration
{
    /// <summary>
    /// Minimum log level to show in the logs.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<LogLevel>))]
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    
    /// <summary>
    /// If true, ASP.NET logging will be enabled.
    /// </summary>
    public bool AspNetLoggingEnabled { get; set; } = false;
}

public class PlaceConfigurationEntry
{
    /// <summary>
    /// API key provided by the client to match.
    /// </summary>
    public string? ApiKey { get; set; } = "default";
    
    /// <summary>
    /// Secret keys used to authenticate requests with HMAC SHA256.
    /// </summary>
    public string? SecretKey { get; set; } = "default";

    /// <summary>
    /// Session cookie used to send shutdown requests.
    /// </summary>
    public string SessionCookie { get; set; } = "ROBLOSECURITY=default";
    
    /// <summary>
    /// List of place ids accepted.
    /// </summary>
    public List<long> PlaceIds { get; set; } = new List<long>();
}

public class RobloxConfiguration
{
    /// <summary>
    /// Places controlled by the application.
    /// </summary>
    public List<PlaceConfigurationEntry> Places { get; set; } = new List<PlaceConfigurationEntry>() { new PlaceConfigurationEntry() };
}

public class ServerConfiguration
{
    /// <summary>
    /// Host to serve on.
    /// </summary>
    public string Host { get; set; } = "localhost";
    
    /// <summary>
    /// Port to serve on.
    /// </summary>
    public ushort Port { get; set; } = 8000;
}

public class Configuration
{
    /// <summary>
    /// Configuration for logging.
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new LoggingConfiguration();
    
    /// <summary>
    /// Configuration for the server.
    /// </summary>
    public ServerConfiguration Server { get; set; } = new ServerConfiguration();
}

[JsonSerializable(typeof(Configuration))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ConfigurationJsonContext : JsonSerializerContext
{
}