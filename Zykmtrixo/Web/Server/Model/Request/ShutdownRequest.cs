using System.Text.Json.Serialization;

namespace Zykmtrixo.Web.Server.Model.Request;

public class ShutdownRequest
{
    /// <summary>
    /// Place id of the server to shut down.
    /// </summary>
    [JsonPropertyName("placeId")]
    public long? PlaceId { get; set; }

    /// <summary>
    /// Job id of the server to shut down.
    /// </summary>
    [JsonPropertyName("jobId")]
    public string? JobId { get; set; } = null!;
}

[JsonSerializable(typeof(ShutdownRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ShutdownRequestJsonContext : JsonSerializerContext
{
}