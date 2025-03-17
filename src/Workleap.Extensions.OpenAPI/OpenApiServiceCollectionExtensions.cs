using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Workleap.Extensions.OpenAPI.Builder;

namespace Workleap.Extensions.OpenAPI;

/// <summary>
/// Provides extension methods to the <see cref="IServiceCollection"/> for configuring OpenAPI/Swagger services.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Configures OpenAPI/Swagger document generation and SwaggerUI.
    /// </summary>
    public static OpenApiBuilder ConfigureOpenApiGeneration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return new OpenApiBuilder(services);
    }

    /// <summary>
    ///  Configure a default JSON serializer options for the application.
    /// </summary>
    /// <param name="mvcBuilder"></param>
    /// <returns></returns>
    public static IMvcBuilder ConfigureStandardJsonSerializerOptions(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.ConfigureAllStandardJsonSerializerOptions();
        return mvcBuilder;
    }

    internal static IServiceCollection ConfigureAllStandardJsonSerializerOptions(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => ConfigureJsonSerializerOptions(options.SerializerOptions));
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => ConfigureJsonSerializerOptions(options.JsonSerializerOptions));

        return services;
    }

    private static void ConfigureJsonSerializerOptions(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.Converters.Add(new JsonStringEnumConverter());
    }
}