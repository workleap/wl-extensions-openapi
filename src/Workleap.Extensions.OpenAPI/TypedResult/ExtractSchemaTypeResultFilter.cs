using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.TypedResult;
// TODO: Deep cleanup redesign of this code (especially check + filtering)
// TODO: Could this be tested with a unit test? (check how Swashbuckle is doing it)
public class ExtractSchemaTypeResultFilter : IOperationFilter
{
    // Accoding to this documentation: https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-8.0
    private static readonly IReadOnlyList<string> DefaultContentTypes = new List<string>() { "application/json", "text/json", "text/plain", };

    // To obtain componenets --> scans all paths. Components is probably extracting from all responses and types.
    // Easy way --> check if annotation present, if present, skip pis defer to annotation.
    // right now, 1. Start by fetching the return type.
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // skip if annotation is present (TODO: WHY?)
        // if(context.MethodInfo.CustomAttributes.Any(x =>x.AttributeType == typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute) || x.AttributeType.BaseType == typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute)))
        // {
        //     return;
        // }

        foreach (var responseMetadata in GetResponsesMetadata(context.MethodInfo.ReturnType))
        {
            // TODO: Required since not nullable?
            operation.Responses ??= new OpenApiResponses();

            if (operation.Responses.TryGetValue(responseMetadata.HttpCode.ToString(), out var existingResponse))
            {
                var canEnrichContent = !existingResponse.Content.Any() && responseMetadata.SchemaType != null;
                if (!canEnrichContent)
                {
                    continue;
                }
            }

            var response = new OpenApiResponse();
            operation.Responses[responseMetadata.HttpCode.ToString()] = response;
            if(responseMetadata.SchemaType != null)
            {
                var schema = context.SchemaGenerator.GenerateSchema(responseMetadata.SchemaType, context.SchemaRepository);

                var contentTypes = GetContentTypes(context);
                foreach (var contentType in contentTypes)
                {
                    response.Content.Add(contentType, new OpenApiMediaType { Schema = schema });
                }
            }
            if (string.IsNullOrEmpty(response.Description)) // TODO: Why set the description? 
            {
                response.Description = responseMetadata.HttpCode.ToString();
            }
        }
    }

    // TODO: Support minimal api (if too complicated: out-of-scoped)
    internal static IReadOnlyCollection<string> GetContentTypes(OperationFilterContext context)
    {
        var methodProducesAttribute = context.MethodInfo.GetCustomAttribute<Microsoft.AspNetCore.Mvc.ProducesAttribute>();
        if (methodProducesAttribute != null)
        {
            return methodProducesAttribute.ContentTypes.ToList();
        }

        var controllerProducesAttribute = context.MethodInfo.DeclaringType?.GetCustomAttribute<Microsoft.AspNetCore.Mvc.ProducesAttribute>();
        if (controllerProducesAttribute != null)
        {
            return controllerProducesAttribute.ContentTypes.ToList();
        }
        
        // (TODO: TO TEST for Minimal API) If the method or controller does not have a ProducesAttribute, check the endpoint metadata
        var endpointProducesAttribute = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<Microsoft.AspNetCore.Mvc.ProducesAttribute>().FirstOrDefault();
        if (endpointProducesAttribute != null)
        {
            return endpointProducesAttribute.ContentTypes.ToList();
        }

        // Fallback on default content types, not supporting globally defined content types
        return DefaultContentTypes;
    }


    internal static IEnumerable<ResponseMetadata> GetResponsesMetadata(Type returnType)
    {
        // Unwrap Task<> to get the return type
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            returnType = returnType.GetGenericArguments()[0];
        }

        if (!typeof(IResult).IsAssignableFrom(returnType))
        {
            yield break;
        }

        var genericTypeCount = returnType.GenericTypeArguments.Length;

        // For type like Ok, BadRequest, NotFound
        if (genericTypeCount == 0)
        {
            // Exclude raw IResult since we can't infer the status code
            if (!typeof(IStatusCodeHttpResult).IsAssignableFrom(returnType))
            {
                yield break;
            }

            var responseMetadata = ExtractMetadataFromTypedResult(returnType);
            yield return responseMetadata;
        }
        // For types like Ok<T>, BadRequest<T>, NotFound<T>
        else if (genericTypeCount == 1)
        {
            var responseMetadata = ExtractMetadataFromTypedResult(returnType);
            yield return responseMetadata;
        }
        // For types like Results<Ok<T>, BadRequest<T>, NotFound<T>>
        else
        {
            foreach (var resultType in returnType.GenericTypeArguments)
            {
                var responseMetadata = ExtractMetadataFromTypedResult(resultType);
                yield return responseMetadata;
            }
        }
    }

    // TODO: Handle nulls: throw or simply return null so it can be ignore (better experience?)
    private static ResponseMetadata ExtractMetadataFromTypedResult(Type resultType)
    {
        // For type like Ok, BadRequest, NotFound
        if (!resultType.GenericTypeArguments.Any())
        {
            var constructor = resultType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
            var instance = constructor.Invoke(Array.Empty<object>());
            var statusCode = (instance as IStatusCodeHttpResult)?.StatusCode;

            return new(statusCode ?? 0, null);
        }
        // For types like Ok<T>, BadRequest<T>, NotFound<T>
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