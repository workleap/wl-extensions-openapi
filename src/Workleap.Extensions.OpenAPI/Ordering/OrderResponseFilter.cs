using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.Ordering;

/// <summary>
///     This filter ensures consistent ordering for better source control diffs and predictable documentation.
/// </summary>
internal sealed class OrderResponseFilter : IDocumentFilter
{
    // In Microsoft.OpenApi v2, operation keys are no longer the OperationType enum (where we could cast to int),
    // they are objects with a string Method property. This dictionary preserves explicit ordering.
    private static readonly Dictionary<string, int> HttpMethodOrder = new(StringComparer.OrdinalIgnoreCase)
    {
        { "GET", 0 },
        { "POST", 1 },
        { "PUT", 2 },
        { "PATCH", 3 },
        { "DELETE", 4 },
        { "OPTIONS", 5 },
        { "HEAD", 6 },
        { "TRACE", 7 }
    };

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        if (document.Paths == null)
        {
            return;
        }

        var paths = document.Paths.ToList();
        document.Paths.Clear();
        document.Paths = new OpenApiPaths();
        foreach (var path in paths)
        {
            document.Paths.Add(path.Key, path.Value);

            if (path.Value is not OpenApiPathItem pathItem || pathItem.Operations == null)
            {
                continue;
            }

            var sortedOperations = pathItem.Operations
                .OrderBy(op => HttpMethodOrder.TryGetValue(op.Key.Method, out var order) ? order : 99)
                .ToList();
            pathItem.Operations.Clear();
            foreach (var operation in sortedOperations)
            {
                pathItem.Operations.Add(operation.Key, operation.Value);

                if (operation.Value is not OpenApiOperation openApiOperation || openApiOperation.Responses == null)
                {
                    continue;
                }

                // Sort responses by status code (200, 400, 403, 404, 500, etc.)
                // This is critical because responses from both controller-level ProducesResponseType
                // and method-level attributes are added in the order they're processed, not by status code.
                // Without sorting, a 403 from a controller-level attribute might appear before a 200
                // from the method-level TypedResult, causing unpredictable ordering and noisy diffs.
                var sortedResponse = openApiOperation.Responses.OrderBy(responseKvp => responseKvp.Key, StringComparer.Ordinal).ToList();
                openApiOperation.Responses.Clear();
                foreach (var response in sortedResponse)
                {
                    openApiOperation.Responses.Add(response.Key, response.Value);
                }
            }
        }
    }
}