using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public static class ExtractTypedResultMinimalApis
{
    public static void AddEndpointsForTypedResult(this WebApplication app)
    {
        app.MapGet("minimal-endpoint-with-typed-result-no-produces", (() => Results.Ok(new TypedResultExample("Example"))))
            .WithName("GetMinimalApiWithName")
            .WithTags("TypedResultWithNoProduces")
            .WithOpenApi();

        app.MapGet("minimal-endpoint-with-typed-result-with-produces", (() => Results.Ok(new ProblemDetails())))
            .WithName("GetMinimalApiWithName")
            .WithTags("TypedResultWithProduces")
            .Produces<TypedResultExample>()
            .WithOpenApi();
    }
}
