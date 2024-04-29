using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace Workleap.Extensions.OpenAPI.TypedResult;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Internal Server Error (500) status code.
/// </summary>
/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Internal Server Error (500) status code.
/// </summary>
public sealed class InternalServerError : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerError"/> class with the values
    /// provided.
    /// </summary>
    internal InternalServerError()
    {
    }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status500InternalServerError"/>
    /// </summary>
    private int StatusCode => StatusCodes.Status500InternalServerError;

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

        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status500InternalServerError, typeof(void)));
    }
}