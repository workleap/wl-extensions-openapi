using System.Text.Json;
using System.Text.Json.Serialization;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Workleap.Extensions.OpenAPI.Builder;

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

    [Fact]
    public async Task WhenDifferentDictionaryKeyJsonSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.KebabCaseLower;
        });
    }

    [Fact]
    public async Task WhenSameDictionaryKeyJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    [Fact]
    public async Task WhenDifferentPropertyNamingPolicyJsonSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;
        });
    }

    [Fact]
    public async Task WhenSamePropertyNamingPolicyJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    [Fact]
    public async Task WhenDifferentDefaultIgnoreConditionSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        });
    }

    [Fact]
    public async Task WhenSameDefaultIgnoreConditionJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        });
    }

    [Fact]
    public async Task WhenDifferentNumberHandlingSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.NumberHandling = JsonNumberHandling.Strict;
        });
    }

    [Fact]
    public async Task WhenSameNumberHandlingJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        });
    }

    [Fact]
    public async Task WhenDifferentRespectNullableAnnotationsSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.RespectNullableAnnotations = false;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.RespectNullableAnnotations = true;
        });
    }

    [Fact]
    public async Task WhenSameRespectNullableAnnotationsJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.RespectNullableAnnotations = true;
        });
    }

    [Fact]
    public async Task WhenDifferentRespectRequiredConstructorParametersJsonSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.RespectRequiredConstructorParameters = false;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.RespectRequiredConstructorParameters = true;
        });
    }

    [Fact]
    public async Task WhenSameRespectRequiredConstructorParametersJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.RespectRequiredConstructorParameters = true;
        });
    }

    [Fact]
    public async Task WhenDifferentIgnoreReadOnlyPropertiesJsonSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.IgnoreReadOnlyProperties = false;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.IgnoreReadOnlyProperties = true;
        });
    }

    [Fact]
    public async Task WhenSameIgnoreReadOnlyPropertiesJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.IgnoreReadOnlyProperties = true;
        });
    }

    [Fact]
    public async Task WhenDifferentIgnoreReadOnlyFieldsJsonSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.IgnoreReadOnlyFields = false;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.IgnoreReadOnlyFields = true;
        });
    }

    [Fact]
    public async Task WhenSameIgnoreReadOnlyFieldsJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.IgnoreReadOnlyFields = true;
        });
    }

    [Fact]
    public async Task WhenIncludeFieldsJsonSerializer_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.IncludeFields = false;
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.IncludeFields = true;
        });
    }

    [Fact]
    public async Task WhenIncludeFieldsJsonSerializer_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.IncludeFields = true;
        });
    }

    [Fact]
    public async Task WhenDifferentConvertersJsonSerializerOptions_FailToRun()
    {
        await this.DifferentJsonOptionsPropertyComparison(httpJsonOptions =>
        {
            httpJsonOptions.Converters.Add(new JsonStringEnumConverter());
        }, mvcJsonOptions =>
        {
            mvcJsonOptions.Converters.Add(new CustomConverter());
        });
    }

    [Fact]
    public async Task WhenSameConvertersJsonSerializerOptions_ThenRuns()
    {
        await this.SameJsonOptionsPropertyComparison(jsonOptions =>
        {
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }

    private async Task DifferentJsonOptionsPropertyComparison(Action<JsonSerializerOptions> httpJsonOptions, Action<JsonSerializerOptions> mvcJsonOptions)
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logger =>
            {
                logger.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                logger.SetMinimumLevel(LogLevel.Debug);
            });
            builder.ConfigureServices(services =>
            {
                services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => httpJsonOptions(options.SerializerOptions));
                services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => mvcJsonOptions(options.JsonSerializerOptions));
            });
        });

        // Act
        Assert.Throws<JsonSerializerDifferenceException>(() => webApplicationFactory.Server);
    }

    private async Task SameJsonOptionsPropertyComparison(Action<JsonSerializerOptions> jsonOptions)
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logger =>
            {
                logger.AddProvider(new XUnitLoggerProvider(testOutputHelper));
                logger.SetMinimumLevel(LogLevel.Debug);
            });
            builder.ConfigureServices(services =>
            {
                services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => jsonOptions(options.SerializerOptions));
                services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => jsonOptions(options.JsonSerializerOptions));
            });
        });
        // Act
        _ = webApplicationFactory.Server; // Force to run the server
    }

    private sealed class CustomConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}