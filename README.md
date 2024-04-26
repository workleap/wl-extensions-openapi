# Workleap.Extensions.OpenAPI

[![nuget](https://img.shields.io/nuget/v/Workleap.Extensions.OpenAPI.svg?logo=nuget)](https://www.nuget.org/packages/Workleap.Extensions.OpenAPI/)
[![build](https://img.shields.io/github/actions/workflow/status/gsoft-inc/wl-extensions-openapi/publish.yml?logo=github&branch=main)](https://github.com/gsoft-inc/wl-extensions-openapi/actions/workflows/publish.yml)

The `Workleap.Extensions.OpenAPI` library is designed to help generate better OpenApi document with less effort.

## Value proposition and features overview

The library offers an opinionated configuration of OpenAPI document generation and SwaggerUI.

As such, we provide the following features:

- Display OperationId in SwaggerUI
- Extract Type schema  from TypedResult endpoint response types 
- (Optional) Fallback to use controller name as OperationId when there is no OperationId explicitly defined for the endpoint.

## Getting started

Install the package Workleap.Extensions.OpenAPI in your .NET API project. Then you may use the following method to register the required service.  Here is a code snippet on how to register this and to enable the operationId fallback feature in your application.

```cs
public void ConfigureServices(IServiceCollection services)
{
  // [...]
  services.ConfigureOpenApiGeneration()
    .GenerateMissingOperationId(); // Optional
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



### Limitations

We currently only support the extraction of the default content types and not any globally defined content types.

## Building, releasing and versioning

The project can be built by running `Build.ps1`. It uses [Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md) to help detect public API breaking changes. Use the built-in roslyn analyzer to ensure that public APIs are declared in `PublicAPI.Shipped.txt`, and obsolete public APIs in `PublicAPI.Unshipped.txt`.

A new *preview* NuGet package is **automatically published** on any new commit on the main branch and whenever we execute the CI pipeline.

When you are ready to **officially release** a stable NuGet package by following the [SemVer guidelines](https://semver.org/), simply **manually create a tag** with the format `x.y.z`. This will automatically create and publish a NuGet package for this version.

## License

Copyright © 2024, Workleap This code is licensed under the Apache License, Version 2.0. You may obtain a copy of this license at [License](https://github.com/gsoft-inc/gsoft-license/blob/master/LICENSE).
