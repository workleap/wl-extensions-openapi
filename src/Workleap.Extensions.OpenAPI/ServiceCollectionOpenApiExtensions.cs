using Microsoft.Extensions.DependencyInjection;

namespace Workleap.Extensions.OpenAPI;

public static class ServiceCollectionOpenApiExtensions
{
    // TODO: Check the setup name
    public static OpenApiBuilder AddOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return new OpenApiBuilder(services);
    }
}