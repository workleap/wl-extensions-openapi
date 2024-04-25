using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

[ApiController]
[Route("typedResult")]
[Produces("application/json")]
[Consumes("application/json")]
public class TypedResultController : ControllerBase
{
    [HttpGet]
    [Route("/withOnlyOnePath")]
    // [ProducesResponseType(typeof(TypedResultExample), StatusCodes.Status200OK)]
    public Ok<TypedResultExample> TypeResultWithOnlyOnePath(int id)
    {
        return TypedResults.Ok(new TypedResultExample("Example"));
    }
    
    [HttpGet]
    [Route("/withNoAnnotation")]
    // [ProducesResponseType(typeof(TypedResultExample), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Results<Ok<TypedResultExample>, BadRequest<ProblemDetails>, NotFound> TypedResultWithNoAnnotation(int id)
    {
        return id switch
        {
            < 0 => TypedResults.NotFound(),
            0 => TypedResults.BadRequest(new ProblemDetails()),
            _ => TypedResults.Ok(new TypedResultExample("Example"))
        };
    }

    [HttpGet]
    [Route("/voidOk")]
    // [ProducesResponseType(typeof(TypedResultExample), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Ok VoidOk(int id)
    {
        return TypedResults.Ok();
    }

    [HttpGet]
    [Route("/withSwaggerResponseAnnotation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns TypedResult", typeof(TypedResultExample), "application/json")]
    public Ok<ProblemDetails> TypedResultWithSwaggerResponseAnnotation()
    {
        return TypedResults.Ok(new ProblemDetails());
    }
    
    [HttpGet]
    [Route("/producesResponseTypeAnnotation")]
    [ProducesResponseType(typeof(TypedResultExample), StatusCodes.Status200OK)]
    public Ok<ProblemDetails> TypedResultWithProducesResponseTypeAnnotation()
    {
        return TypedResults.Ok(new ProblemDetails());
    }
}