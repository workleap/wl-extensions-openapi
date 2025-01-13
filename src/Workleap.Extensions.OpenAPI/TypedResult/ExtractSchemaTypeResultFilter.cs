using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.TypedResult;

internal sealed class ExtractSchemaTypeResultFilter : IOperationFilter
{
    // Based on this documentation: https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-8.0
    private const string DefaultContentType = MediaTypeNames.Application.Json;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // If the endpoint has annotations defined, we don't want to remove them.
        var explicitlyDefinedResponseCodes = ExtractResponseCodesFromAttributes(context.MethodInfo.CustomAttributes);

        var usesTypedResultsReturnType = false;
        foreach (var responseMetadata in GetResponsesMetadata(context.MethodInfo.ReturnType))
        {
            explicitlyDefinedResponseCodes.Add(responseMetadata.HttpCode);
            // If the response content is already set, we won't overwrite it. This is the case for minimal APIs and
            // when the ProducesResponseType attribute is present.
            if (operation.Responses.TryGetValue(responseMetadata.HttpCode.ToString(), out var existingResponse))
            {
                // If no content type is specified, three will be added by default: application/json, text/plain, and text/json.
                // In this case we want to enforce the application/json content type.
                if (IsDefaultContentTypes(existingResponse.Content))
                {
                    existingResponse.Content.Clear();
                }

                var canEnrichContent = !existingResponse.Content.Any() && responseMetadata.SchemaType != null;

                if (!canEnrichContent)
                {
                    continue;
                }
            }

            usesTypedResultsReturnType = true;
            var response = new OpenApiResponse();
            if (HttpResultsStatusCodeTypeHelpers.StatusCodesToDescription.TryGetValue(responseMetadata.HttpCode, out var description))
            {
                response.Description = description;
            }
            else
            {
                response.Description = responseMetadata.HttpCode.ToString();
            }

            if (responseMetadata.SchemaType != null)
            {
                var schema = context.SchemaGenerator.GenerateSchema(responseMetadata.SchemaType, context.SchemaRepository);
                response.Content.Add(DefaultContentType, new OpenApiMediaType { Schema = schema });
            }

            operation.Responses[responseMetadata.HttpCode.ToString()] = response;
        }

        // The spec is generated with a default 200 response, we need to remove it if the endpoint does not return 200.
        if (usesTypedResultsReturnType && !explicitlyDefinedResponseCodes.Contains(200))
        {
            operation.Responses.Remove("200");
        }
    }

    private static bool IsDefaultContentTypes(IDictionary<string, OpenApiMediaType> contentTypes) =>
        contentTypes.ContainsKey(MediaTypeNames.Application.Json) &&
        contentTypes.ContainsKey(MediaTypeNames.Text.Plain) &&
        contentTypes.ContainsKey("text/json");

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

    internal static HashSet<int> ExtractResponseCodesFromAttributes(IEnumerable<CustomAttributeData> customAttributes)
    {
        HashSet<int> responseCodes = [];
        foreach (var attribute in customAttributes)
        {
            if (!typeof(ProducesResponseTypeAttribute).IsAssignableFrom(attribute.AttributeType))
            {
                continue;
            }

            foreach (var argument in attribute.ConstructorArguments)
            {
                if (argument.Value is int httpCode)
                {
                    responseCodes.Add(httpCode);
                }
            }
        }

        return responseCodes;
    }

    // Initialize an instance of the result type to get the response metadata and return null if it's not possible
    private static ResponseMetadata? ExtractMetadataFromTypedResult(Type resultType)
    {
        if (ExtractStatusCodeFromType(resultType) is not { } statusCode)
        {
            return null;
        }

        // For type like Ok, BadRequest, NotFound
        if (resultType.GenericTypeArguments.Length == 0)
        {
            return new(statusCode, null);
        }

        // For types like Ok<T>, BadRequest<T>, NotFound<T>
        return new(statusCode, resultType.GenericTypeArguments.First());
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