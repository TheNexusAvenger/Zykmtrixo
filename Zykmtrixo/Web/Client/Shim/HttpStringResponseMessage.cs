using System.Collections.Generic;
using System.Net;

namespace Zykmtrixo.Web.Client.Shim;

public class HttpStringResponseMessage
{
    /// <summary>
    /// Status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Headers of the response.
    /// All headers will be lowercase.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = null!;
    
    /// <summary>
    /// Body of the response.
    /// </summary>
    public string Content { get; set; } = null!;
}