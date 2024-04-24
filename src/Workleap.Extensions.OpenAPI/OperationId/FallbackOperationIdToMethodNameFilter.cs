using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.OperationId;

internal class FallbackOperationIdToMethodNameFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.IsNullOrEmpty(operation.OperationId))
        {
            return;
        }

        // Method name for Minimal API is not the best choice for OperationId so we want to enforce explicit declaration
        if (IsMinimalApi(context))
        {
            return;
        }

        operation.OperationId = CleanupName(context.MethodInfo.Name);
    }

    private static bool IsMinimalApi(OperationFilterContext context)
    {
        return !typeof(ControllerBase).IsAssignableFrom(context.MethodInfo.DeclaringType);
    }

    internal static string CleanupName(string methodName)
    {
        return methodName.Replace("Async", string.Empty, StringComparison.InvariantCultureIgnoreCase);
    }
}