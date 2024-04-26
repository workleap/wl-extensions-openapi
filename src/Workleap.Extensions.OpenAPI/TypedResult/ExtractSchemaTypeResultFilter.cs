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
        
        foreach (var responseMetadata in GetResponsesMetadata(context.MethodInfo.ReturnType))
        {
            if (!operation.Responses.TryGetValue(responseMetadata.HttpCode.ToString(), out var response))
            {
                response = new OpenApiResponse();
            }

            operation.Responses[responseMetadata.HttpCode.ToString()] = response;
            if(responseMetadata.SchemaType != null)
            {
                var schema = context.SchemaGenerator.GenerateSchema(responseMetadata.SchemaType, context.SchemaRepository);

                response.Content.Add("application/json", new OpenApiMediaType { Schema = schema });
            }
            if (string.IsNullOrEmpty(response.Description))
            {
                response.Description = responseMetadata.HttpCode.ToString();
            }
        }
    }

    internal static IEnumerable<ResponseMetadata> GetResponsesMetadata(Type returnType)
    {
        if (typeof(IResult).IsAssignableFrom(returnType))
        {
            yield break;
        }

        var genericTypeCount = returnType.GenericTypeArguments.Length;

        if (genericTypeCount == 0)
        {
            var responseMetadata = ExtractResponseMetadata(returnType);
            yield return responseMetadata;
        }
        else if (genericTypeCount == 1)
        {
            var responseMetadata = ExtractResponseMetadata(returnType);
            yield return responseMetadata;
        }
        else
        {
            foreach (var resultType in returnType.GenericTypeArguments)
            {
                var responseMetadata = ExtractResponseMetadata(resultType);
                yield return responseMetadata;
            }
        }
    }

    // TODO: Handle nulls
    private static ResponseMetadata ExtractResponseMetadata(Type resultType)
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
            var statusCode = (instance as IStatusCodeHttpResult)?.StatusCode;

            return new(statusCode ?? 0, null);
        } 
        // I am declaring a return type with a schema
        else
        {
            var constructor = resultType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {resultType.GenericTypeArguments.First() }, null);
            var instance = constructor.Invoke(new object[] { null });
            var statusCode = (instance as IStatusCodeHttpResult)?.StatusCode;
            return new(statusCode ?? 0, resultType.GenericTypeArguments.First());
        }
    }
    
    internal class ResponseMetadata
    {
        public ResponseMetadata(int httpCode, Type? schemaType)
        {
            this.HttpCode = httpCode;
            this.SchemaType = schemaType;
        }

        public int HttpCode { get; set; }
        public Type? SchemaType { get; set; }
    }
}