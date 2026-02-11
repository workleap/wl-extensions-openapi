using System.Reflection;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.RequiredType;

// If a Data type property is not nullable then it has to be required. This filter defaults the properties to be required unless marked as nullable.
// https://swagger.io/docs/specification/data-models/data-types/
internal sealed class ExtractRequiredAttributeFromNullableType : ISchemaFilter
{
#if NET10_0_OR_GREATER
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema || openApiSchema.Properties == null || openApiSchema.Required == null)
        {
            return;
        }

        // For .NET 10 / OpenAPI 3.1 / Swashbuckle v10, the SupportNonNullableReferenceTypes extension
        // handles nullability differently. We'll apply a simpler approach here.
        // In OpenAPI 3.1, nullable is typically represented through anyOf/oneOf with null type.

        var nullabilityInfoContext = new NullabilityInfoContext();
        var contextProperties = context.Type.GetProperties();

        foreach (var (name, property) in openApiSchema.Properties)
        {
            if (property is not OpenApiSchema propSchema)
            {
                continue;
            }

            // Check if already marked as required
            if (openApiSchema.Required.Contains(name))
            {
                continue;
            }

            // Try to determine if the property is non-nullable using reflection
            var contextProperty = contextProperties.FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase));
            if (contextProperty == null)
            {
                continue;
            }

            var nullabilityInfo = nullabilityInfoContext.Create(contextProperty);
            // If the property is non-nullable, add it to required
            if (nullabilityInfo.ReadState == NullabilityState.NotNull)
            {
                openApiSchema.Required.Add(name);
            }
        }
    }
#else
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
        {
            return;
        }

        PatchNonNullableReferenceTypesOnNestedSchema(schema, context);

        // This is used in conjunction with the SupportNonNullableReferenceTypes extension which uses the C# nullable feature to set properties as nullable.
        var notNullableProperties = schema
            .Properties
            .Where(property => !property.Value.Nullable && !schema.Required.Contains(property.Key))
            .ToList();

        foreach (var property in notNullableProperties)
        {
            schema.Required.Add(property.Key);
        }
    }

    // There is a bug on where SupportNonNullableReferenceTypes does not work for nested record types. This method is a workaround to fix the issue.
    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2758
    private static void PatchNonNullableReferenceTypesOnNestedSchema(OpenApiSchema schema, SchemaFilterContext context)
    {
        // NullabilityInfoContext is used to analyze the nullability of properties. It uses reflection to inspect the type of the member and determine if it is nullable.
        var nullabilityInfoContext = new NullabilityInfoContext();
        var contextProperties = context.Type.GetProperties();

        foreach (var (name, property) in schema.Properties)
        {
            var contextProperty = contextProperties.FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase));
            if (contextProperty is null)
            {
                continue;
            }

            var nullabilityInfo = nullabilityInfoContext.Create(contextProperty);
            // If nullability is unknown or ambiguous, we continue.
            if (nullabilityInfo is { ReadState: NullabilityState.Unknown, WriteState: NullabilityState.Unknown } || nullabilityInfo.ReadState != nullabilityInfo.WriteState)
            {
                continue;
            }

            // If there is a mismatch between the OpenApiSchema nullability and the context reflected nullability, we defer to the context nullability.
            var detectedNullability = property.Nullable;
            var reflectedNullability = nullabilityInfo.ReadState == NullabilityState.Nullable;
            if (detectedNullability != reflectedNullability)
            {
                property.Nullable = reflectedNullability;
            }
        }
    }
#endif
}