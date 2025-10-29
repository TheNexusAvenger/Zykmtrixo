using System.Security.Cryptography;
using System.Text;
using Zykmtrixo.Web.Server.Model.Request;

namespace Zykmtrixo.Test.Web.Server.Model.Request;

public class RequestContextTest
{
    [Test]
    public void TestIsAuthorizedUnsplittable()
    {
        var requestContext = new RequestContext("TestAuthorization", "{}");
        Assert.That(requestContext.IsAuthorized("TestAuthorization", null), Is.False);
    }
    
    [Test]
    public void TestIsAuthorizedApiKey()
    {
        var requestContext = new RequestContext("ApiKey TestAuthorization", "{}");
        Assert.That(requestContext.IsAuthorized("TestAuthorization", null), Is.True);
    }
    
    [Test]
    public void TestIsAuthorizedBearer()
    {
        var requestContext = new RequestContext("Bearer TestAuthorization", "{}");
        Assert.That(requestContext.IsAuthorized("TestAuthorization", null), Is.True);
    }
    
    [Test]
    public void TestIsAuthorizedBearerInvalid()
    {
        var requestContext = new RequestContext("Bearer OtherAuthorization", "{}");
        Assert.That(requestContext.IsAuthorized("TestAuthorization", null), Is.False);
    }
    
    [Test]
    public void TestIsAuthorizedSignatureInvalid()
    {
        var requestContext = new RequestContext($"Signature Invalid", "{}");
        Assert.That(requestContext.IsAuthorized(null, "TestSignature"), Is.False);
    }
    
    [Test]
    public void TestIsAuthorizedSignatureMatch()
    {
        using var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes("TestSignature"));
        var signature = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes("{}")));
        var requestContext = new RequestContext($"Signature {signature}", "{}");
        Assert.That(requestContext.IsAuthorized(null, "TestSignature"), Is.True);
    }
}