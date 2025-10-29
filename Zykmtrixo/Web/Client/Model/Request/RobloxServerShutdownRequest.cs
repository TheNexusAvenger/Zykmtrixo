using System.Text.Json.Serialization;

namespace Zykmtrixo.Web.Client.Model.Request;

public class RobloxServerShutdownRequest
{
    /// <summary>
    /// Place id of the server to shut down.
    /// </summary>
    [JsonPropertyName("placeId")]
    public long PlaceId { get; set; }

    /// <summary>
    /// Job id of the server to shut down.
    /// </summary>
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = null!;
}

[JsonSerializable(typeof(RobloxServerShutdownRequest))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class RobloxServerShutdownRequestJsonContext : JsonSerializerContext
{
}