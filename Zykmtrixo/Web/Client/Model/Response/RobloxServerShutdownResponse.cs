using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zykmtrixo.Web.Client.Model.Response;

public class RobloxErrorEntry
{
    /// <summary>
    /// Code for the error.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Message for the error.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}

public class RobloxServerShutdownResponse
{
    /// <summary>
    /// Place id of the server that was shut down.
    /// </summary>
    [JsonPropertyName("placeId")]
    public long? PlaceId { get; set; }
    
    /// <summary>
    /// Job id of the server that was shut down.
    /// </summary>
    [JsonPropertyName("gameId")]
    public string? GameId { get; set; }
    
    /// <summary>
    /// Errors for the response.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<RobloxErrorEntry>? Errors { get; set; }
}

[JsonSerializable(typeof(RobloxServerShutdownResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class RobloxServerShutdownResponseJsonContext : JsonSerializerContext
{
}