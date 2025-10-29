using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Zykmtrixo.Web.Server.Model.Response;

public class BasicResponse : BaseResponse
{
    /// <summary>
    /// Creates a basic response.
    /// </summary>
    /// <param name="status">Status of the response.</param>
    public BasicResponse(ResponseStatus status)
    {
        this.Status = status;
    }
    
    /// <summary>
    /// Returns the JSON type information of the response.
    /// </summary>
    /// <returns>The JSON type information of the response.</returns>
    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return BasicResponseJsonContext.Default.BasicResponse;
    }
}

[JsonSerializable(typeof(BasicResponse))]
[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class BasicResponseJsonContext : JsonSerializerContext
{
}