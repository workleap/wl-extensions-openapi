using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.RequiredType;

// If a Data type property is not nullable then it has to be required. This filter defaults the properties to be required unless marked as nullable.
// https://swagger.io/docs/specification/data-models/data-types/
internal sealed class ExtractRequiredAttributeFromNullableType : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
        {
            return;
        }

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
}