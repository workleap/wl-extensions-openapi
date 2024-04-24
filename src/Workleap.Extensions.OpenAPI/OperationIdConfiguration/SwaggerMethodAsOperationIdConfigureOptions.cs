using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Workleap.Extensions.OpenAPI;

// TODO : SHould it be an OperationFilter instead??
internal class SwaggerMethodAsOperationIdConfigureOptions: IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        // options.CustomOperationIds(apiDesc =>
        // {
        //     var operationId = HasExplicitOperationId(apiDesc);
        //     if (!string.IsNullOrEmpty(operationId))
        //     {
        //         return operationId;
        //     }
        //
        //     return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
        // });
        
        // First it will run the DefaultOperationIdSelector to get from the method
        // We can't really garantee order of  OperationFilter if ours will run before or after the Swagger Annotation one, but theirs always overwrite so we are good (AnnotationsOperationFilter)
        // Unless we take care of configuring EnableAnnotation()
        
//         private string DefaultOperationIdSelector(ApiDescription apiDescription)
//         {
//             var actionDescriptor = apiDescription.ActionDescriptor;
//
//             // Resolve the operation ID from the route name and fallback to the
//             // endpoint name if no route name is available. This allows us to
//             // generate operation IDs for endpoints that are defined using
//             // minimal APIs.
// #if (!NETSTANDARD2_0)
//             return
//                 actionDescriptor.AttributeRouteInfo?.Name
//                 ?? (actionDescriptor.EndpointMetadata?.LastOrDefault(m => m is IEndpointNameMetadata) as IEndpointNameMetadata)?.EndpointName;
// #else
//             return actionDescriptor.AttributeRouteInfo?.Name;
// #endif
//         }

        
        options.OperationFilter<SwaggerDefaultOperationIdToMethodNameFilter>();
    }

    private string? HasExplicitOperationId(ApiDescription apiDescription)
    {
        var attributeName = apiDescription.ActionDescriptor.AttributeRouteInfo.Name;
        if (!string.IsNullOrEmpty(attributeName))
        {
            return attributeName;
        }

        var swaggerOperationAttribute = apiDescription.ActionDescriptor.EndpointMetadata.OfType<SwaggerOperationAttribute>().FirstOrDefault();
        if (swaggerOperationAttribute != null && !string.IsNullOrEmpty(swaggerOperationAttribute.OperationId))
        {
            return swaggerOperationAttribute.OperationId;
        }

        return null;
    }
}