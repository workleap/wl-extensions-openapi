# Workleap.Extensions.OpenAPI

[![nuget](https://img.shields.io/nuget/v/Workleap.Extensions.OpenAPI.svg?logo=nuget)](https://www.nuget.org/packages/Workleap.Extensions.OpenAPI/)
[![build](https://img.shields.io/github/actions/workflow/status/gsoft-inc/wl-extensions-openapi/publish.yml?logo=github&branch=main)](https://github.com/gsoft-inc/wl-extensions-openapi/actions/workflows/publish.yml)

## Getting started

This library provides helpers and guidelines for API endpoints annotations during OpenAPI spec generation. As such, we provide the following features:

- Fallback to use controller name as OperationId when there is no OperationId explicitly defined for the endpoint.

### How to use it

Install the package Workleap.Extensions.OpenAPI in your .NET API project. Then you may use the following method to register the required service.  Here is a code snippet on how to register this and to enable the operationId fallback feature in your application.

```
public void ConfigureServices(IServiceCollection services)
{
  // [...]
  services.AddOpenApi().FallbackOnMethodNameForOperationId();
  // [...]
}
```

## Building, releasing and versioning

The project can be built by running `Build.ps1`. It uses [Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md) to help detect public API breaking changes. Use the built-in roslyn analyzer to ensure that public APIs are declared in `PublicAPI.Shipped.txt`, and obsolete public APIs in `PublicAPI.Unshipped.txt`.

A new *preview* NuGet package is **automatically published** on any new commit on the main branch and whenever we execute the CI pipeline.

When you are ready to **officially release** a stable NuGet package by following the [SemVer guidelines](https://semver.org/), simply **manually create a tag** with the format `x.y.z`. This will automatically create and publish a NuGet package for this version.


## License

Copyright © 2024, Workleap This code is licensed under the Apache License, Version 2.0. You may obtain a copy of this license at https://github.com/gsoft-inc/gsoft-license/blob/master/LICENSE.
