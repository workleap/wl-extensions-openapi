using Microsoft.Extensions.DependencyInjection;

namespace Workleap.Extensions.OpenAPI;

/// <summary>
/// TODO
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static OpenApiBuilder AddOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return new OpenApiBuilder(services);
    }
}