using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI.Ordering;

internal class OrderResponseFilter: IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        var orderedPaths = document.Paths.OrderBy(pathKvp => pathKvp.Key, StringComparer.Ordinal).ToList();
        document.Paths.Clear();
        document.Paths = new OpenApiPaths();
        foreach (var path in orderedPaths)
        {
            document.Paths.Add(path.Key, path.Value);

            var sortedOperations = path.Value.Operations.OrderBy(op => (int)op.Key).ToList();
            path.Value.Operations.Clear();
            foreach (var operation in sortedOperations)
            {
                path.Value.Operations.Add(operation.Key, operation.Value);

                // Sort response test
                var sortedResponse = operation.Value.Responses.OrderBy(responseKvp => responseKvp.Key, StringComparer.Ordinal).ToList();
                operation.Value.Responses.Clear();
                foreach (var response in sortedResponse)
                {
                    operation.Value.Responses.Add(response.Key, response.Value);
                }

                // Sort parameters (TODO: CHECK!)
                if (operation.Value.Parameters != null)
                {
                    var sortedParameters = operation.Value.Parameters.OrderBy(param => param.Name, StringComparer.Ordinal).ToList();
                    operation.Value.Parameters.Clear();
                    foreach (var parameter in sortedParameters)
                    {
                        operation.Value.Parameters.Add(parameter);
                    }
                }
            }
        }
    }
}