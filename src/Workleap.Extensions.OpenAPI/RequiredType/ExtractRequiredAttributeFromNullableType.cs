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
        if (schema is not OpenApiSchema openApiSchema || openApiSchema.Properties == null)
        {
            return;
        }

        PatchNonNullableReferenceTypesOnNestedSchema(openApiSchema, context);

        // This is used in conjunction with the SupportNonNullableReferenceTypes extension which uses the C# nullable feature to set properties as nullable.
        // In OpenAPI 3.1 (Swashbuckle v10), nullable is handled differently - we check if the property allows null values
        var notNullableProperties = openApiSchema
            .Properties
            .Where(property => property.Value is OpenApiSchema propSchema && !IsNullableType(propSchema) && !openApiSchema.Required.Contains(property.Key))
            .ToList();

        foreach (var property in notNullableProperties)
        {
            openApiSchema.Required.Add(property.Key);
        }
    }

    private static bool IsNullableType(OpenApiSchema schema)
    {
        // In OpenAPI 3.1, nullable is typically represented through anyOf/oneOf combinations with null type
        // For now, we'll check if the schema explicitly allows null through various mechanisms
        if (schema.AnyOf != null && schema.AnyOf.Count > 0)
        {
            return schema.AnyOf.Any(s => s is OpenApiSchema os && IsNullSchema(os));
        }

        if (schema.OneOf != null && schema.OneOf.Count > 0)
        {
            return schema.OneOf.Any(s => s is OpenApiSchema os && IsNullSchema(os));
        }

        // Check if Type property indicates nullable - note that Type in v2 is different
        return false;
    }

    private static bool IsNullSchema(OpenApiSchema schema)
    {
        // Check if this is a null type schema
        return schema.Type == "null" || (schema.Enum != null && schema.Enum.Count == 1 && schema.Enum[0] == null);
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
            if (property is not OpenApiSchema openApiProperty)
            {
                continue;
            }

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

            // For .NET 10 / OpenAPI 3.1, we skip patching nullability as it's handled differently
            // The Swashbuckle.AspNetCore v10 SupportNonNullableReferenceTypes should handle this correctly
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