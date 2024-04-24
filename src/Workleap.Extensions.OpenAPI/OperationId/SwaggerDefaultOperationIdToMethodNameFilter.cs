using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI;

internal class SwaggerDefaultOperationIdToMethodNameFilter: IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.IsNullOrEmpty(operation.OperationId))
        {
            return;
        }

        // Remove Async
        // Should we provide an extension points to customize the name?
        operation.OperationId = context.MethodInfo.Name;
    }
}