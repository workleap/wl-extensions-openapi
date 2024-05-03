using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.RequiredType;

internal sealed class SetPropertyAsRequiredTypeFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
        {
            return;
        }

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