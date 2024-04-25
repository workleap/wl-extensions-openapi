using Microsoft.AspNetCore.Mvc;
using WebApi.OpenAPI.SystemTest.ExtractTypeResult;

namespace WebApi.OpenAPI.SystemTest.OperationId;

public static class OperationIdMinimalApis
{
    public static void AddEndpointsForOperationId(this WebApplication app)
    {
        // app.MapGet("minimal-endpoint-with-name", (() => TypedResults.Ok()))
        //     .WithName("GetMinimalApiWithName")
        //     .WithTags("ExtractTypeResult")
        //     .WithOpenApi();
        //
        // app.MapGet("minimal-endpoint-with-no-name", () => Results.Ok())
        //     .WithTags("ExtractTypeResult")
        //     .WithOpenApi();
    }
}
