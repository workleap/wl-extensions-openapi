using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using Workleap.Extensions.OpenAPI.OperationId;
using Workleap.Extensions.OpenAPI.RequiredType;
using Workleap.Extensions.OpenAPI.TypedResult;

namespace Workleap.Extensions.OpenAPI.Builder;

/// <summary>
/// Provides methods to configure Swagger/OpenAPI opinionated settings for the application.
/// </summary>
public sealed class OpenApiBuilder
{
    private readonly IServiceCollection _services;

    internal OpenApiBuilder(IServiceCollection services)
    {
        this._services = services;

        this._services.AddSingleton<IConfigureOptions<SwaggerUIOptions>, DisplayOperationIdInSwaggerUiOptions>();
        this._services.ConfigureSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();
            options.OperationFilter<ExtractSchemaTypeResultFilter>();
            options.SchemaFilter<ExtractRequiredAttributeFromNullableType>();
        });
    }

    /// <summary>
    /// Configures the Swagger generator to fallback on the method name as the operation ID if no explicit operation ID is specified.
    /// </summary>
    /// <remarks>
    /// This method adds a custom operation filter to the Swagger generator.
    /// </remarks>
    /// <returns>
    /// The same <see cref="OpenApiBuilder"/> instance so that multiple configuration calls can be chained.
    /// </returns>
    public OpenApiBuilder GenerateMissingOperationId()
    {
        this._services.ConfigureSwaggerGen(options =>
        {
            options.OperationFilter<FallbackOperationIdToMethodNameFilter>();
        });

        return this;
    }

    public OpenApiBuilder ConfigureStandardJsonSerializerOptions()
    {
        this._services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => this.ConfigureJsonOptions(options.SerializerOptions));
        this._services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => this.ConfigureJsonOptions(options.JsonSerializerOptions));

        return this;
    }

    private void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.Converters.Add(new JsonStringEnumConverter());
    }
}