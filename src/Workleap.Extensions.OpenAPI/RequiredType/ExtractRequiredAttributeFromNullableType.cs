using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.RequiredType;

// If a Data type property is not nullable then it has to be required. This filter defaults the properties to be required unless marked as nullable.
// https://swagger.io/docs/specification/data-models/data-types/
internal sealed class ExtractRequiredAttributeFromNullableType : ISchemaFilter
{
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
}