# Workleap.Extensions.OpenAPI

[![nuget](https://img.shields.io/nuget/v/Workleap.Extensions.OpenAPI.svg?logo=nuget)](https://www.nuget.org/packages/Workleap.Extensions.OpenAPI/)
[![build](https://img.shields.io/github/actions/workflow/status/workleap/wl-extensions-openapi/publish.yml?logo=github&branch=main)](https://github.com/workleap/wl-extensions-openapi/actions/workflows/publish.yml)

The `Workleap.Extensions.OpenAPI` library is designed to help generate better OpenApi document with less effort.

## Value proposition and features overview

The library offers an opinionated configuration of OpenAPI document generation and SwaggerUI.

As such, we provide the following features:

OpenAPI Spec generation filters:
- Display OperationId in SwaggerUI
- Extract Type schema from TypedResult endpoint response types.
- Ensure that non-nullable properties are marked as required in the OpenAPI document. It is no longer necessary to add `[Required]` attributes to object properties.
- (Optional) Fallback to use controller name as OperationId when there is no OperationId explicitly defined for the endpoint.

Roslyn Analyzers to help validate usage typed responses:
- Rule to catch mismatches between endpoint response annotations and Typed Responses
- Rule to help enforce usage of strongly typed responses

## Getting started

Install the package Workleap.Extensions.OpenAPI in your .NET API project. Then you may use the following method to register the required service.  Here is a code snippet on how to register this and to enable the operationId fallback feature in your application.

```cs
public void ConfigureServices(IServiceCollection services)
{
  // [...]
  services.ConfigureOpenApiGeneration()
    .GenerateMissingOperationId() // Optional
    .ConfigureStandardJsonSerializerOptions(); //Optional
}
```

We support the extraction of Response Types automatically. For example, considering the following API code snippet:
```cs
[HttpGet]
[Route("/get-example/{id}")]
public Results<Ok<TypedResultExample>, BadRequest<ProblemDetails>, NotFound> GetExample(int id)
{
    return id switch
    {
        < 0 => TypedResults.NotFound(),
        0 => TypedResults.BadRequest(new ProblemDetails()),
        _ => TypedResults.Ok(new TypedResultExample("Example"))
    };
}
```

The resulting OpenAPI snippet would be generated:
```yaml
  /get-example:
    get:
      tags:
        - TypedResult
      operationId: TypedResultWithNoAnnotation2
      parameters:
        - name: id
          in: query
          style: form
          schema:
            type: integer
            format: int32
      responses:
        '200':
          description: '200'
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/TypedResultExample'
        '400':
          description: '400'
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ProblemDetails'
        '404':
          description: '404'
```

One thing to note is that the library does not overwrite any explicitly defined ResponseType attributes on the endpoint. For example, considering the endpoints below, despite having `ProblemDetails` being defined as the TypedResult return, the schema will defer to the `TypedResultExample` schema given that it is explicitly defined in the SwaggerResponse or ProducesResponseType attributes. 

```cs
[HttpGet]
[Route("/withSwaggerResponseAnnotation")]
[SwaggerResponse(StatusCodes.Status200OK, "Returns TypedResult", typeof(TypedResultExample), "application/json")] 
// The OpenAPI document would be generated with the TypedResultExample schema rather than ProblemDetails as per signature. 
public Ok<ProblemDetails> TypedResultWithSwaggerResponseAnnotation()
{
    return TypedResults.Ok(new ProblemDetails());
}

[HttpGet]
[Route("/producesResponseTypeAnnotation")]
[ProducesResponseType(typeof(TypedResultExample), StatusCodes.Status200OK)] 
// The OpenAPI document would be generated with the TypedResultExample schema rather than ProblemDetails as per signature.
public Ok<ProblemDetails> TypedResultWithProducesResponseTypeAnnotation()
{
    return TypedResults.Ok(new ProblemDetails());
}
```

### Note: TypedResults requires `Microsoft.AspNetCore.Http.Json.JsonOptions` to be configured

TypedResults uses obtains the JsonOptions from `Microsoft.AspNetCore.Http.Json.JsonOptions`. If you are using IActionResult return types, typically, you would configure `Microsoft.AspNetCore.Mvc.JsonOptions` which also configure the JsonSerializerOptions for SwashBuckle. To use this library, you need to configure both JsonOptions.

We offer an extension method which contains a default configuration of both methods through `ConfigureStandardJsonOptions()`.
```cs
services.ConfigureOpenApiGeneration()
    .GenerateMissingOperationId() // Optional
    .ConfigureStandardJsonSerializerOptions(); //Optional

/*
The default configuration is equivalent to:

options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
options.Converters.Add(new JsonStringEnumConverter());
*/
```


If you would like to configure JsonOptions different, then you can register them as your own extension method. Here is a snippet of code demonstrating that:

```cs
// API Extension methods

// SwashBuckle and TypedResults require different JsonOptions to be configured.
// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2293
public static IServiceCollection ConfigureJsonSerializerOptions(this IServiceCollection services)
{
    services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => ConfigureJsonOptions(options.SerializerOptions));
    services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => ConfigureJsonOptions(options.JsonSerializerOptions));

    return services;
}

private static void ConfigureJsonOptions(JsonSerializerOptions options)
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.Converters.Add(new JsonStringEnumConverter());
}
```


## Included Roslyn analyzers

| Rule ID | Category | Severity | Description                                                        |
|---------|----------|----------|--------------------------------------------------------------------|
| WLOAS001 | Design  | Warning  | Mismatch between annotation return type and endpoint return type. |
| WLOAS002 | Usage  | Warning  | Enforce strongly typed endpoint response. |

To modify the severity of one of these diagnostic rules, you can use a `.editorconfig` file. For example:

```ini
## Disable analyzer for test files
[**Tests*/**.cs]
dotnet_diagnostic.WLOAS001.severity = none
dotnet_diagnostic.WLOAS002.severity = none
```

To learn more about configuring or suppressing code analysis warnings, refer to [this documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/suppress-warnings).

### `WLOAS001`: Mismatch between annotation return type and endpoint return type.

This rule validates the return type indicated by the endpoint annotations against the Typed response values indicated by the endpoint. Here is an example:

```cs
[HttpGet]
[Route("/example")]
[ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // This would be marked with a warning given typeof(string) is different from typeof(TypedResultExample)
[ProducesResponseType(typeof(TypedResultExample), StatusCodes.Status200OK)] // This would be valid
public Ok<TypedResultExample> TypedResultExample()
{
    return TypedResults.Ok(new TypedResultExample());
}
```

### `WLOAS002`: Enforce strongly typed endpoint response.

This rule enforces the usage of strongly type responses for endpoints. The usage of weakly response types such as `IActionResult` and `IResult` would be marked with warnings. Here is an example of a warning:

```cs
[HttpGet]
[Route("/example")]
public IActionResult EnforceStronglyTypedResponse() //This is not a strongly typed response and would be marked with a warning
{
    return TypedResults.Ok(new TypedResultExample());
}
```

Here is an example of a strongly typed response:

```cs
[HttpGet]
[Route("/example")]
public Ok<TypedResultExample> EnforceStronglyTypedResponse() //This is a strongly typed response 
{
    return TypedResults.Ok(new TypedResultExample());
}
```

### Limitations

[Given that HttpResults return types do not leverage the configured Formatters](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-8.0#httpresults-type), content negotiation is not supported for these endpoints and the produced Content-Type is decided by HttpResults implementation. For our use cases, it will be `application/json`.


## Building, releasing and versioning

The project can be built by running `Build.ps1`. It uses [Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md) to help detect public API breaking changes. Use the built-in roslyn analyzer to ensure that public APIs are declared in `PublicAPI.Shipped.txt`, and obsolete public APIs in `PublicAPI.Unshipped.txt`.

A new *preview* NuGet package is **automatically published** on any new commit on the main branch and whenever we execute the CI pipeline.

When you are ready to **officially release** a stable NuGet package by following the [SemVer guidelines](https://semver.org/), simply **manually create a tag** with the format `x.y.z`. This will automatically create and publish a NuGet package for this version.

## License

Copyright © 2024, Workleap This code is licensed under the Apache License, Version 2.0. You may obtain a copy of this license at [License](https://github.com/workleap/gsoft-license/blob/master/LICENSE).
