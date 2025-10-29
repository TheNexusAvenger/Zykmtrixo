using System.Net;
using Zykmtrixo.State;
using Zykmtrixo.Test.Web.Client.Shim;
using Zykmtrixo.Web.Client;
using Zykmtrixo.Web.Client.Shim;
using Zykmtrixo.Web.Server.Controller;
using Zykmtrixo.Web.Server.Model.Request;
using Zykmtrixo.Web.Server.Model.Response;

namespace Zykmtrixo.Test.Web.Server.Controller;

public class ShutdownControllerTest
{
    private TestHttpClient _testClient;
    private ShutdownController _shutdownController;
    
    [SetUp]
    public void SetUp()
    {
        this._testClient = new TestHttpClient();
        RobloxUserClient.HttpClient = this._testClient;

        ConfigurationState.Instance.CurrentConfiguration.Roblox.Places = new List<PlaceConfigurationEntry>()
        {
            new PlaceConfigurationEntry()
            {
                ApiKey = "TestAuthorization",
                SessionCookie = "TestSessionCookie",
                PlaceIds = new List<long>() { 12345 },
            }
        };
        
        this._shutdownController = new ShutdownController();
    }

    [Test]
    public void TestHandleShutdownRequestMalformedJson()
    {
        var request = new RequestContext("Bearer TestAuthorization", "{");
        var response = this._shutdownController.HandleShutdownRequest(request).Result;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.Unauthorized));
    }

    [Test]
    public void TestHandleShutdownRequestMissingPlaceId()
    {
        var request = new RequestContext("Bearer TestAuthorization", "{\"jobId\":\"TestJobId\"}");
        var response = this._shutdownController.HandleShutdownRequest(request).Result;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.Unauthorized));
    }

    [Test]
    public void TestHandleShutdownRequestMissingJobId()
    {
        var request = new RequestContext("Bearer TestAuthorization", "{\"placeId\":12345}");
        var response = this._shutdownController.HandleShutdownRequest(request).Result;
        Assert.That(response.StatusCode, Is.EqualTo(400));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.MalformedRequest));
    }

    [Test]
    public void TestHandleShutdownRequestUnconfigured()
    {
        var request = new RequestContext("Bearer TestAuthorization", "{\"placeId\":23456,\"jobId\":\"TestJobId\"}");
        var response = this._shutdownController.HandleShutdownRequest(request).Result;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.Unauthorized));
    }

    [Test]
    public void TestHandleShutdownRequestUnauthorized()
    {
        var request = new RequestContext("Bearer OtherAuthorization", "{\"placeId\":12345,\"jobId\":\"TestJobId\"}");
        var response = this._shutdownController.HandleShutdownRequest(request).Result;
        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.Unauthorized));
    }

    [Test]
    public void TestHandleShutdownRequest()
    {
        var totalRequests = 0;
        this._testClient.SetResponseResolver("https://apis.roblox.com/matchmaking-api/v1/game-instances/shutdown", (requesst) =>
        {
            totalRequests += 1;
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Headers = new Dictionary<string, string>(),
                Content = "{}",
            };
        });
        
        var request = new RequestContext("Bearer TestAuthorization", "{\"placeId\":12345,\"jobId\":\"TestJobId\"}");
        var response = this._shutdownController.HandleShutdownRequest(request).Result;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.Success));
        Assert.That(totalRequests, Is.EqualTo(1));
    }
}