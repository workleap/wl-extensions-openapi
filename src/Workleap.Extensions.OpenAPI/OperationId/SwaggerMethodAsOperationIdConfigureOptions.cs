using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI;

// TODO : SHould it be an OperationFilter instead??
internal class SwaggerMethodAsOperationIdConfigureOptions: IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.OperationFilter<SwaggerDefaultOperationIdToMethodNameFilter>();
    }
}