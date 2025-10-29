using System.Net;
using Zykmtrixo.State;
using Zykmtrixo.Test.Web.Client.Shim;
using Zykmtrixo.Web.Client;
using Zykmtrixo.Web.Client.Shim;
using Zykmtrixo.Web.Server.Controller;
using Zykmtrixo.Web.Server.Model.Response;

namespace Zykmtrixo.Test.Web.Client.Controller;

public class HealthControllerTest
{
    private TestHttpClient _testClient;
    private HealthController _healthController;
    
    [SetUp]
    public void SetUp()
    {
        this._testClient = new TestHttpClient();
        RobloxUserClient.HttpClient = this._testClient;

        ConfigurationState.Instance.CurrentConfiguration.Roblox.Places = new List<PlaceConfigurationEntry>()
        {
            new PlaceConfigurationEntry()
            {
                SessionCookie = "TestSessionCookie",
                PlaceIds = new List<long>() { 12345 },
            }
        };
        
        this._healthController = new HealthController();
    }

    [Test]
    public void TestHandleHealthCheckSuccess()
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

        var response = this._healthController.HandleHealthCheck().Result;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.Success));
        Assert.That(totalRequests, Is.EqualTo(1));

        // It should be cached.
        response = this._healthController.HandleHealthCheck().Result;
        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.Success));
        Assert.That(totalRequests, Is.EqualTo(1));
    }

    [Test]
    public void TestHandleHealthCheckError()
    {
        var totalRequests = 0;
        this._testClient.SetResponseResolver("https://apis.roblox.com/matchmaking-api/v1/game-instances/shutdown", (requesst) =>
        {
            totalRequests += 1;
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Headers = new Dictionary<string, string>(),
                Content = "{}",
            };
        });

        var response = this._healthController.HandleHealthCheck().Result;
        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.ServerError));
        Assert.That(totalRequests, Is.EqualTo(1));
        
        // It should be cached.
        response = this._healthController.HandleHealthCheck().Result;
        Assert.That(response.StatusCode, Is.EqualTo(503));
        Assert.That(response.Response.Status, Is.EqualTo(ResponseStatus.ServerError));
        Assert.That(totalRequests, Is.EqualTo(1));
    }
}