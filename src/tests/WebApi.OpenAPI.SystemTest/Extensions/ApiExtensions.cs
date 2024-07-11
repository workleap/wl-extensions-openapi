using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApi.OpenAPI.SystemTest.Extensions;

public static class ApiExtensions
{
    public static void ConfigureControllers(this IServiceCollection services)
    {
        services.AddControllers();
        services.ConfigureJsonSerializerOptions();
    }

    // SwashBuckle and TypedResults require different JsonOptions to be configured.
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2293
    private static IServiceCollection ConfigureJsonSerializerOptions(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => ConfigureJsonOptions(options.SerializerOptions));
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => ConfigureJsonOptions(options.JsonSerializerOptions));

        return services;
    }

    private static void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.WriteIndented = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.Converters.Add(new JsonStringEnumConverter());
    }
}