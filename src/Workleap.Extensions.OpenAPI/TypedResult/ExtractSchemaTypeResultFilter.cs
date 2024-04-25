using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.TypedResult;
// TODO: Deep cleanup redesign of this code (especially check + filtering)
public class ExtractSchemaTypeResultFilter : IOperationFilter
{
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
            var (httpCode, responseType) = this.ExtractResponseType(context.MethodInfo.ReturnType);
            yield return (httpCode, responseType);
        }
        else
        {
            foreach (var resultType in responseTypes)
            {
                var (httpCode, responseType) = this.ExtractResponseType(resultType);
                yield return (httpCode, responseType);
            }
        }
    }
    
    private (string HttpCode, Type SchemaType) ExtractResponseType(Type resultType)
    {
        if (!typeof(IResult).IsAssignableFrom(resultType))
        {
            // TODO: Means it's not a result type (maybe not strong enough check IActionResult,...)
            throw new Exception();
        }

        // I am declaring a return type that return void
        if (!resultType.GenericTypeArguments.Any())
        {
            var constructor = resultType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
            var instance = constructor.Invoke(Array.Empty<object>());
            var statusCode = (instance as IStatusCodeHttpResult)?.StatusCode.ToString();

            return (statusCode, null);
        } 
        else
        {
            var constructor = resultType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {resultType.GenericTypeArguments.First() }, null);
            var instance = constructor.Invoke(new object[] { null });
            var statusCode = (instance as IStatusCodeHttpResult)?.StatusCode.ToString();
            return (statusCode, resultType.GenericTypeArguments.First());
        }
    }
}