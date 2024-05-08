using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace Workleap.Extensions.OpenAPI.TypedResult;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Forbidden (403) status code.
/// </summary>
public sealed class Forbidden : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Forbidden"/> class with the values
    /// provided.
    /// </summary>
    internal Forbidden()
    {
    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status403Forbidden"/>
    /// </summary>
    private int StatusCode => StatusCodes.Status403Forbidden;

    int? IStatusCodeHttpResult.StatusCode => this.StatusCode;

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        httpContext.Response.StatusCode = this.StatusCode;

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status403Forbidden, typeof(void)));
    }
}