using Microsoft.Extensions.DependencyInjection;

namespace Workleap.Extensions.OpenAPI;

public static class ServiceCollectionOpenApiExtensions
{
    // TODO: Check the setup name
    public static OpenApiBuilder AddOpenApi(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        return new OpenApiBuilder(services);
    }
}