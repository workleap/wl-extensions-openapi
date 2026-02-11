#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Workleap.Extensions.OpenAPI;

namespace WebApi.OpenAPI.SystemTest;

public static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        // Required to detect Minimal Api Endpoints
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Test API", Version = "v1" });
            options.EnableAnnotations();
        });

        services.ConfigureOpenApiGeneration()
            .GenerateMissingOperationId()
            .ConfigureStandardJsonSerializerOptions();

        return services;
    }
}