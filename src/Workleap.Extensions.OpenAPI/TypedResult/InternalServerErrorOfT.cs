using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace Workleap.Extensions.OpenAPI.TypedResult;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Internal Server Error (500) status code.
/// </summary>
/// <typeparam name="TValue">The type of error object that will be JSON serialized to the response body.</typeparam>
#pragma warning disable SA1649 // File name should match first type name - This is a Generic class.
public sealed class InternalServerError<TValue> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
#pragma warning restore SA1649
    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerError"/> class with the values
    /// provided.
    /// </summary>
    /// <param name="error">The error content to format in the entity body.</param>
    internal InternalServerError(TValue? error)
    {
        this.Value = error;
        if (this.Value is ProblemDetails problemDetails)
        {
            problemDetails.Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1";
            problemDetails.Title = "An error occurred while processing your request.";
        }
    }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; }

    object? IValueHttpResult.Value => this.Value;

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status500InternalServerError"/>
    /// </summary>
    private static int StatusCode => StatusCodes.Status500InternalServerError;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    private static readonly string[] ContentTypes = new[] { "application/json" };

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = StatusCode;

        return httpContext.Response.WriteAsJsonAsync(
            this.Value);
    }

    /// <inheritdoc/>
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status500InternalServerError, typeof(TValue), ContentTypes));
    }
}