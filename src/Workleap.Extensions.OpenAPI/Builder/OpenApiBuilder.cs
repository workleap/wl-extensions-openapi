using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Workleap.Extensions.OpenAPI;

/// <summary>
/// Provides methods to configure Swagger/OpenAPI opinionated settings for the application.
/// </summary>
public class OpenApiBuilder
{
    private readonly IServiceCollection _services;

    internal OpenApiBuilder(IServiceCollection services)
    {
        this._services = services;

        this._services.AddSingleton<IConfigureOptions<SwaggerUIOptions>, DisplayOperationIdInSwaggerUiOptions>();
    }

    /// <summary>
    /// Configures the Swagger generator to fallback on the method name as the operation ID if no explicit operation ID is specified.
    /// </summary>
    /// <remarks>
    /// This method adds a custom operation filter to the Swagger generator.
    /// </remarks>
    /// <returns>
    /// The same <see cref="OpenApiBuilder"/> instance so that multiple configuration calls can be chained.
    /// </returns>
    public OpenApiBuilder FallbackOnMethodNameForOperationId()
    {
        this._services.ConfigureSwaggerGen(options =>
        {
            options.OperationFilter<FallbackOperationIdToMethodNameFilter>();
        });

        return this;
    }
}