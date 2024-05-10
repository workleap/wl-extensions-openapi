using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.TypedResult;

internal sealed class ExtractSchemaTypeResultFilter : IOperationFilter
{
    // Based on this documentation: https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-8.0
    private const string DefaultContentType = "application/json";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var responseMetadata in GetResponsesMetadata(context.MethodInfo.ReturnType))
        {
            // If the response content is already set, we won't overwrite it. This is the case for minimal APIs and
            if (operation.Responses.TryGetValue(responseMetadata.HttpCode.ToString(), out var existingResponse))
            {
                var canEnrichContent = !existingResponse.Content.Any() && responseMetadata.SchemaType != null;

                // when the ProducesResponseType attribute is present.
                if (!canEnrichContent)
                {
                    continue;
                }
            }

            var response = new OpenApiResponse();
            response.Description = responseMetadata.HttpCode.ToString();

            if (responseMetadata.SchemaType != null)
            {
                var schema = context.SchemaGenerator.GenerateSchema(responseMetadata.SchemaType, context.SchemaRepository);
                response.Content.Add(DefaultContentType, new OpenApiMediaType { Schema = schema });
            }

            operation.Responses[responseMetadata.HttpCode.ToString()] = response;
        }
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
            if (!typeof(IStatusCodeHttpResult).IsAssignableFrom(returnType))
            {
                yield break;
            }

            var responseMetadata = ExtractMetadataFromTypedResult(returnType);
            if (responseMetadata != null)
            {
                yield return responseMetadata;
            }
        }
        // For types like Ok<T>, BadRequest<T>, NotFound<T>
        else if (genericTypeCount == 1)
        {
            var responseMetadata = ExtractMetadataFromTypedResult(returnType);
            if (responseMetadata != null)
            {
                yield return responseMetadata;
            }
        }
        // For types like Results<Ok<T>, BadRequest<T>, NotFound<T>>
        else
        {
            foreach (var resultType in returnType.GenericTypeArguments)
            {
                var responseMetadata = ExtractMetadataFromTypedResult(resultType);
                if (responseMetadata != null)
                {
                    yield return responseMetadata;
                }
            }
        }
    }

    // Initialize an instance of the result type to get the response metadata and return null if it's not possible
    private static ResponseMetadata? ExtractMetadataFromTypedResult(Type resultType)
    {
        if (ExtractStatusCodeFromType(resultType) is not { } statusCode)
        {
            return null;
        }

        // For type like Ok, BadRequest, NotFound
        if (!resultType.GenericTypeArguments.Any())
        {
            return new(statusCode, null);
        }
        // For types like Ok<T>, BadRequest<T>, NotFound<T>
        else
        {
            return new(statusCode, resultType.GenericTypeArguments.First());
        }
    }

    private static int? ExtractStatusCodeFromType(Type resultType)
    {
        var typeString = $"{resultType.Namespace}.{resultType.Name}";
        if (HttpResultsStatusCodeTypeHelpers.HttpResultTypeToStatusCodes.TryGetValue(typeString, out var statusCode))
        {
            return statusCode;
        }

        return null;
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