using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public static class ExtractTypedResultMinimalApis
{
    public static void AddEndpointsForTypedResult(this WebApplication app)
    {
        app.MapGet("minimal-endpoint-with-typed-result-no-produces/{id:int}", CheckReturnById)
            .WithName("GetMinimalApiWithTypedResultWithNoProduces")
            .WithTags("TypedResult")
            .WithOpenApi();

        app.MapGet("minimal-endpoint-with-typed-result-with-produces", CheckReturnByIdWithProblemDetails)
            .WithName("GetMinimalApiWithTypedResultWithProduces")
            .WithTags("TypedResult")
            .Produces<TypedResultExample>()
            .Produces<ProblemDetails>(400)
            .WithOpenApi();
    }

    private static Results<Ok<TypedResultExample>, BadRequest<ProblemDetails>, NotFound> CheckReturnById(int id)
    {
        return id switch
        {
            > 0 => TypedResults.NotFound(),
            0 => TypedResults.BadRequest(new ProblemDetails()),
            _ => TypedResults.Ok(new TypedResultExample("Example"))
        };
    }

    private static Results<Ok<ProblemDetails>, BadRequest<ProblemDetails>, NotFound> CheckReturnByIdWithProblemDetails(int id)
    {
        return id switch
        {
            > 0 => TypedResults.NotFound(),
            0 => TypedResults.BadRequest(new ProblemDetails()),
            _ => TypedResults.Ok(new ProblemDetails())
        };
    }
}
