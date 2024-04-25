using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.TypedResult;

public class ExtractSchemaTypeResultFilter : IOperationFilter
{
    // To obtain componenets --> scans all paths. Components is probably extracting from all responses and types.
    // Easy way --> check if annotation present, if present, skip pis defer to annotation.
    // right now, 1. Start by fetching the return type.
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var statusCode = "200";
        
        if (operation.Responses == null)
        {
            operation.Responses = new OpenApiResponses();
        }
        
        if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse response))
        {
            response = new OpenApiResponse();
        }
        
        operation.Responses[statusCode] = response;
        var schema = context.SchemaGenerator.GenerateSchema(context.MethodInfo.ReturnType.GenericTypeArguments.FirstOrDefault(), context.SchemaRepository);
        
        response.Content.Add("application/json", new OpenApiMediaType { Schema = schema });
    }
}