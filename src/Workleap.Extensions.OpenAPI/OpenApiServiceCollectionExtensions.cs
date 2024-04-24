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
    public static OpenApiBuilder AddOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return new OpenApiBuilder(services);
    }
}