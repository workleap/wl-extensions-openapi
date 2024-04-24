using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Workleap.Extensions.OpenAPI;

// TODO: Check what should be sealed or not
// TODO: Check if should have interface
public class OpenApiBuilder
{
    private readonly IServiceCollection _services;

    public OpenApiBuilder(IServiceCollection services)
    {
        this._services = services;
        
        // TODO: Maybe not do in the constructor
        // Transiant or singleton?
        this._services.AddTransient<IConfigureOptions<SwaggerUIOptions>, DisplayOperationIdInSwaggerUIOptions>();
    }

    public OpenApiBuilder FallbackOnMethodNameForOperationId()
    {
        this._services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerMethodAsOperationIdConfigureOptions>();
        return this;
    }
}