using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.Ordering;

internal class OrderResponseFilter: IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        var paths = document.Paths.ToList();
        document.Paths.Clear();
        document.Paths = new OpenApiPaths();
        foreach (var path in paths)
        {
            document.Paths.Add(path.Key, path.Value);

            var sortedOperations = path.Value.Operations.OrderBy(op => (int)op.Key).ToList();
            path.Value.Operations.Clear();
            foreach (var operation in sortedOperations)
            {
                path.Value.Operations.Add(operation.Key, operation.Value);

                var sortedResponse = operation.Value.Responses.OrderBy(responseKvp => responseKvp.Key, StringComparer.Ordinal).ToList();
                operation.Value.Responses.Clear();
                foreach (var response in sortedResponse)
                {
                    operation.Value.Responses.Add(response.Key, response.Value);
                }
            }
        }
    }
}