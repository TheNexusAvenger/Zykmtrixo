using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Zykmtrixo.Web.Server.Model.Response;

public enum ResponseStatus
{
    // Generic success response.
    Success,
    
    // Generic client error response.
    MalformedRequest,
    Unauthorized,
    
    // Generic server error response.
    ServerError,
}

public abstract class BaseResponse
{
    /// <summary>
    /// Status of the response.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ResponseStatus>))]
    [JsonPropertyName("status")]
    public ResponseStatus Status { get; set; } = ResponseStatus.Success;

    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public abstract JsonTypeInfo GetJsonTypeInfo();
}