using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public static class ExtractTypedResultMinimalApis
{
    public static void AddEndpointsForTypedResult(this WebApplication app)
    {
#if NET10_0_OR_GREATER
        app.MapGet("minimal-endpoint-with-typed-result-no-produces/{id:int}", CheckReturnById)
            .WithName("GetMinimalApiWithTypedResultWithNoProduces")
            .WithTags("TypedResult");

        app.MapGet("minimal-endpoint-with-typed-result-with-produces", CheckReturnByIdWithProblemDetails)
            .WithName("GetMinimalApiWithTypedResultWithProduces")
            .WithTags("TypedResult")
            .Produces<TypedResultExample>()
            .Produces<ProblemDetails>(400);
#else
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
#endif
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
