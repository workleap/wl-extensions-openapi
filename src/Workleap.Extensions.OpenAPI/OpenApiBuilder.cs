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
        
        // Transiant or singleton?
        this._services.AddTransient<IConfigureOptions<SwaggerUIOptions>, DisplayOperationIdOptions>();
    }

    public OpenApiBuilder UseMethodNameAsOperationId()
    {
        this._services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerMethodAsOperationIdConfigureOptions>();
        return this;
    }
}