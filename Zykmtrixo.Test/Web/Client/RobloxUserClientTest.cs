using System.Net;
using System.Text.Json;
using Zykmtrixo.State;
using Zykmtrixo.Test.Web.Client.Shim;
using Zykmtrixo.Web.Client;
using Zykmtrixo.Web.Client.Model.Request;
using Zykmtrixo.Web.Client.Shim;

namespace Zykmtrixo.Test.Web.Client;

public class RobloxUserClientTest
{
    private TestHttpClient _testClient;
    private RobloxUserClient _robloxUserClient;
    
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

        this._robloxUserClient = new RobloxUserClient(12345);
    }

    [Test]
    public void TestShutDownAsyncFirstResponse()
    {
        var totalRequests = 0;
        this._testClient.SetResponseResolver("https://apis.roblox.com/matchmaking-api/v1/game-instances/shutdown", (request) =>
        {
            var requestData = JsonSerializer.Deserialize<RobloxServerShutdownRequest>(request.Content!.ReadAsStream(),
                RobloxServerShutdownRequestJsonContext.Default.RobloxServerShutdownRequest)!;
            Assert.That(requestData.PlaceId, Is.EqualTo(12345));
            Assert.That(requestData.GameId, Is.EqualTo("TestJobId"));
            
            totalRequests += 1;
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Headers = new Dictionary<string, string>()
                {
                    {"x-csrf-token", "TestToken"},
                },
                Content = "{\"placeId\":12345}",
            };
        });
        
        var response = _robloxUserClient.ShutDownAsync("TestJobId").Result;
        Assert.That(response.PlaceId, Is.EqualTo(12345));
        Assert.That(totalRequests, Is.EqualTo(1));
    }

    [Test]
    public void TestShutDownAsyncSecondResponse()
    {
        var totalRequests = 0;
        this._testClient.SetResponseResolver("https://apis.roblox.com/matchmaking-api/v1/game-instances/shutdown", (request) =>
        {
            var requestData = JsonSerializer.Deserialize<RobloxServerShutdownRequest>(request.Content!.ReadAsStream(),
                RobloxServerShutdownRequestJsonContext.Default.RobloxServerShutdownRequest)!;
            Assert.That(requestData.PlaceId, Is.EqualTo(12345));
            Assert.That(requestData.GameId, Is.EqualTo("TestJobId"));

            totalRequests += 1;
            if (totalRequests == 1)
            {
                return new HttpStringResponseMessage()
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Headers = new Dictionary<string, string>()
                    {
                        {"x-csrf-token", "TestToken"},
                    },
                    Content = "{\"errors\":[]}",
                };
            }
            
            Assert.That(request.Headers.GetValues("x-csrf-token").First(), Is.EqualTo("TestToken"));
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Headers = new Dictionary<string, string>()
                {
                    {"x-csrf-token", "TestToken"},
                },
                Content = "{\"placeId\":12345}",
            };
        });
        
        var response = _robloxUserClient.ShutDownAsync("TestJobId").Result;
        Assert.That(response.PlaceId, Is.EqualTo(12345));
        Assert.That(totalRequests, Is.EqualTo(2));
    }

    [Test]
    public void TestShutDownAsyncErrorFirstTime()
    {
        var totalRequests = 0;
        this._testClient.SetResponseResolver("https://apis.roblox.com/matchmaking-api/v1/game-instances/shutdown", (request) =>
        {
            var requestData = JsonSerializer.Deserialize<RobloxServerShutdownRequest>(request.Content!.ReadAsStream(),
                RobloxServerShutdownRequestJsonContext.Default.RobloxServerShutdownRequest)!;
            Assert.That(requestData.PlaceId, Is.EqualTo(12345));
            Assert.That(requestData.GameId, Is.EqualTo("TestJobId"));

            totalRequests += 1;
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Headers = new Dictionary<string, string>()
                {
                    {"x-csrf-token", "TestToken"},
                },
                Content = "{\"errors\":[]}",
            };
        });

        var exception = Assert.Throws<AggregateException>(() => _robloxUserClient.ShutDownAsync("TestJobId").Wait());
        var innerException = (HttpRequestException) exception.InnerException!;
        Assert.That(innerException.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(totalRequests, Is.EqualTo(1));
    }

    [Test]
    public void TestShutDownAsyncCsrfErrorSecondTime()
    {
        var totalRequests = 0;
        this._testClient.SetResponseResolver("https://apis.roblox.com/matchmaking-api/v1/game-instances/shutdown", (request) =>
        {
            var requestData = JsonSerializer.Deserialize<RobloxServerShutdownRequest>(request.Content!.ReadAsStream(),
                RobloxServerShutdownRequestJsonContext.Default.RobloxServerShutdownRequest)!;
            Assert.That(requestData.PlaceId, Is.EqualTo(12345));
            Assert.That(requestData.GameId, Is.EqualTo("TestJobId"));

            totalRequests += 1;
            return new HttpStringResponseMessage()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Headers = new Dictionary<string, string>()
                {
                    {"x-csrf-token", "TestToken"},
                },
                Content = "{\"errors\":[]}",
            };
        });

        var exception = Assert.Throws<AggregateException>(() => _robloxUserClient.ShutDownAsync("TestJobId").Wait());
        var innerException = (HttpRequestException) exception.InnerException!;
        Assert.That(innerException.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(totalRequests, Is.EqualTo(2));
    }
}