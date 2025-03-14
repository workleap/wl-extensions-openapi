using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace Workleap.Extensions.OpenAPI.Tests.TypedResult;

public class JsonSerializationOptionsTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task WhenCallEnumEndpoint_ReturnsOkResultWithEnumAsString()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logger =>
            {
                logger.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                logger.SetMinimumLevel(LogLevel.Debug);
            });
        });

        // Act
        var client = webApplicationFactory.CreateClient();
        var response = await client.GetStringAsync("/withEnum");

        // Assert
        Assert.Equal("""{"name":"Example","count":0,"operation":"Foobar"}""", response);
    }
}