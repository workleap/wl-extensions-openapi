using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.TypedResult;

public class ExtractSchemaTypeResultFilter : IOperationFilter
{
    public static Dictionary<string, string> NameStatusCodeDict { get; set; } = new Dictionary<string, string>
        {
            {"Ok`1", "200"},
            {"BadRequest`1", "400"},
            {"NotFound", "404"},
        };
    // To obtain componenets --> scans all paths. Components is probably extracting from all responses and types.
    // Easy way --> check if annotation present, if present, skip pis defer to annotation.
    // right now, 1. Start by fetching the return type.
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null)
        {
            operation.Responses = new OpenApiResponses();
        }
        
        // skip if annotation is present
        if(context.MethodInfo.CustomAttributes.Any(x =>x.AttributeType == typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute) || x.AttributeType.BaseType == typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute)))
        {
            return;
        }
        
        foreach (var (statusCode, type) in this.GetResponseTypes(context))
        {
            if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse? response))
            {
                response = new OpenApiResponse();
            }
            
            operation.Responses[statusCode] = response;
            if(type != null)
            {
                var schema = context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
                
                response.Content.Add("application/json", new OpenApiMediaType { Schema = schema });    
            }
            if (string.IsNullOrEmpty(response.Description))
            {
                response.Description = statusCode;
            }
        }
    }
    
    private IEnumerable<(string, Type)> GetResponseTypes(OperationFilterContext context)
    {
        var responseTypes = context.MethodInfo.ReturnType.GenericTypeArguments;
        if (responseTypes.Length == 1 && !responseTypes.First().IsGenericType)
        {
            yield return (NameStatusCodeDict[context.MethodInfo.ReturnType.Name], responseTypes.First());   
        }
        else 
        {
            foreach (var responseType in responseTypes)
            {
                yield return (NameStatusCodeDict[responseType.Name], responseType.GenericTypeArguments.FirstOrDefault())!;
            }
        }
        // if(context.MethodInfo.ReturnType.IsGenericType && context.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(IValueHttpResult<>))
        // {
        //     var responseReturn = context.MethodInfo.ReturnType as IStatusCodeHttpResult;
        //     yield return (responseReturn!.StatusCode.ToString(), context.MethodInfo.ReturnType.GenericTypeArguments.FirstOrDefault())!;
        // }
    } 
}