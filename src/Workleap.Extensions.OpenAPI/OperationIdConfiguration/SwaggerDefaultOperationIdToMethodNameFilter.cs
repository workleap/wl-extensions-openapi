using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI;

public class SwaggerDefaultOperationIdToMethodNameFilter: IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.IsNullOrEmpty(operation.OperationId))
        {
            return;
        }

        operation.OperationId = context.MethodInfo.Name;
    }
}