using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Workleap.Extensions.OpenAPI;

internal class DisplayOperationIdInSwaggerUIOptions: IConfigureOptions<SwaggerUIOptions>
{
    public void Configure(SwaggerUIOptions options)
    {
        options.DisplayOperationId();
    }
}