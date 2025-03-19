using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        this._services.AddSingleton<IStartupFilter, JsonOptionsFilter>();
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

    /// <summary>
    /// Configures the default Json serializer options used for OpenAPI and controllers
    /// </summary>
    /// <returns></returns>
    public OpenApiBuilder ConfigureStandardJsonSerializerOptions()
    {
        this._services.ConfigureAllStandardJsonSerializerOptions();

        return this;
    }

    private sealed class JsonOptionsFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                var mvcJsonOptions = builder.ApplicationServices.GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>().Value;
                var httpJsonOptions = builder.ApplicationServices.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>().Value;

                if (mvcJsonOptions.JsonSerializerOptions.DictionaryKeyPolicy != httpJsonOptions.SerializerOptions.DictionaryKeyPolicy)
                {
                    throw new JsonSerializerDifferenceException("Property name");
                }

                if (mvcJsonOptions.JsonSerializerOptions.PropertyNamingPolicy != httpJsonOptions.SerializerOptions.PropertyNamingPolicy)
                {
                    throw new JsonSerializerDifferenceException("Dictionary key");
                }

                if (mvcJsonOptions.JsonSerializerOptions.DefaultIgnoreCondition != httpJsonOptions.SerializerOptions.DefaultIgnoreCondition)
                {
                    throw new JsonSerializerDifferenceException("Default ignore condition");
                }

                if (mvcJsonOptions.JsonSerializerOptions.NumberHandling != httpJsonOptions.SerializerOptions.NumberHandling)
                {
                    throw new JsonSerializerDifferenceException("Property name");
                }

                if (mvcJsonOptions.JsonSerializerOptions.RespectNullableAnnotations != httpJsonOptions.SerializerOptions.RespectNullableAnnotations)
                {
                    throw new JsonSerializerDifferenceException("Dictionary key");
                }

                if (mvcJsonOptions.JsonSerializerOptions.RespectRequiredConstructorParameters != httpJsonOptions.SerializerOptions.RespectRequiredConstructorParameters)
                {
                    throw new JsonSerializerDifferenceException("Default ignore condition");
                }

                if (mvcJsonOptions.JsonSerializerOptions.IgnoreReadOnlyProperties != httpJsonOptions.SerializerOptions.IgnoreReadOnlyProperties)
                {
                    throw new JsonSerializerDifferenceException("Property name");
                }

                if (mvcJsonOptions.JsonSerializerOptions.IgnoreReadOnlyFields != httpJsonOptions.SerializerOptions.IgnoreReadOnlyFields)
                {
                    throw new JsonSerializerDifferenceException("Dictionary key");
                }

                if (mvcJsonOptions.JsonSerializerOptions.IncludeFields != httpJsonOptions.SerializerOptions.IncludeFields)
                {
                    throw new JsonSerializerDifferenceException("Default ignore condition");
                }

                if (!CompareConverters(mvcJsonOptions.JsonSerializerOptions.Converters, httpJsonOptions.SerializerOptions.Converters))
                {
                    throw new JsonSerializerDifferenceException("Converters");
                }

                next(builder);
            };
        }

        private static bool CompareConverters(IList<JsonConverter>? left, IList<JsonConverter>? right)
        {
            // equates null with empty lists
            if (left is null)
            {
                return right is null || right.Count == 0;
            }

            if (right is null)
            {
                return left.Count == 0;
            }

            int n;
            if ((n = left.Count) != right.Count)
            {
                return false;
            }

            for (var i = 0; i < n; i++)
            {
                if (left[i].Type != right[i].Type)
                {
                    return false;
                }
            }

            return true;
        }
    }
}