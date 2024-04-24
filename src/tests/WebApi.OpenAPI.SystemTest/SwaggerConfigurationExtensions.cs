using Workleap.Extensions.OpenAPI;

namespace WebApi.OpenAPI.SystemTest;

public static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(x =>
        {
            x.EnableAnnotations();
        });

        services.AddOpenApi()
            .FallbackOnMethodNameForOperationId();

        return services;
    }
}