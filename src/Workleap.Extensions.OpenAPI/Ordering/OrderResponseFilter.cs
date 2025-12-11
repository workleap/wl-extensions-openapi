using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.Ordering;

/// <summary>
///     This filter ensures consistent ordering for better source control diffs and predictable documentation.
/// </summary>
internal class OrderResponseFilter : IDocumentFilter
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

                // Sort responses by status code (200, 400, 403, 404, 500, etc.)
                // This is critical because responses from both controller-level ProducesResponseType
                // and method-level attributes are added in the order they're processed, not by status code.
                // Without sorting, a 403 from a controller-level attribute might appear before a 200
                // from the method-level TypedResult, causing unpredictable ordering and noisy diffs.
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