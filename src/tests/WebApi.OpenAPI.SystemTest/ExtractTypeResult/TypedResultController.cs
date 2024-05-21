using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Workleap.Extensions.OpenAPI.TypedResult;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

[ApiController]
[Route("typedResult")]
[Produces("application/json")]
[Consumes("application/json")]
public class TypedResultController : ControllerBase
{
    [HttpGet]
    [Route("/withOnlyOnePath")]
    public Ok<TypedResultExample> TypeResultWithOnlyOnePath(int id)
    {
        return TypedResults.Ok(new TypedResultExample("Example"));
    }

    [HttpGet]
    [Route("/withAnnotation")]
    [ProducesResponseType(typeof(TypedResultExample), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Results<Ok<ProblemDetails>, BadRequest<ProblemDetails>, NotFound> TypedResultWithAnnotation(int id)
    {
        return id switch
        {
            < 0 => TypedResults.NotFound(),
            0 => TypedResults.BadRequest(new ProblemDetails()),
            _ => TypedResults.Ok(new ProblemDetails())
        };
    }

    [HttpGet]
    [Route("/withNoAnnotation")]
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
    [Route("/withNoAnnotationForAcceptedAndUnprocessableResponseNoType")]
    public Results<Ok<TypedResultExample>, Accepted, UnprocessableEntity> TypedResultWithNoAnnotationForAcceptedAndUnprocessableResponseNoType(int id)
    {
        return id switch
        {
            < 0 => TypedResults.UnprocessableEntity(),
            0 => TypedResults.Ok(new TypedResultExample("Example")),
            _ => TypedResults.Accepted("hardcoded uri")
        };
    }

    [HttpGet]
    [Route("/withNoAnnotationForAcceptedAndUnprocessableResponseWithType")]
    public Results<Ok<TypedResultExample>, Accepted<TypedResultExample>, UnprocessableEntity<TypedResultExample>> TypedResultWithNoAnnotationForAcceptedAndUnprocessableResponseWithType(int id)
    {
        return id switch
        {
            < 0 => TypedResults.UnprocessableEntity(new TypedResultExample("Example")),
            0 => TypedResults.Ok(new TypedResultExample("Example")),
            _ => TypedResults.Accepted("hardcoded uri", new TypedResultExample("example"))
        };
    }

    [HttpGet]
    [Route("/withNoAnnotationForCreatedAndConflictNoType")]
    public Results<Ok<TypedResultExample>, Created, Conflict> TypedResultWithNoAnnotationForCreatedAndConflictNoType(int id)
    {
        return id switch
        {
            < 0 => TypedResults.Conflict(),
            0 => TypedResults.Ok(new TypedResultExample("Example")),
            _ => TypedResults.Created()
        };
    }

    [HttpGet]
    [Route("/withNoAnnotationForCreatedAndConflictWithType")]
    public Results<Ok<TypedResultExample>, Created<string>, Conflict<string>> TypedResultWithNoAnnotationForCreatedAndConflictWithType(int id)
    {
        return id switch
        {
            < 0 => TypedResults.Conflict("Conflict"),
            0 => TypedResults.Ok(new TypedResultExample("Example")),
            _ => TypedResults.Created("hardcoded uri", "Created")
        };
    }

    [HttpGet]
    [Route("/withNoAnnotationForNoContentAndUnauthorized")]
    public Results<NoContent, UnauthorizedHttpResult> TypedResultWithNoAnnotationForNoContentAndUnauthorized(int id)
    {
        return id switch
        {
            < 0 => TypedResults.NoContent(),
            _ => TypedResults.Unauthorized()
        };
    }

    [HttpGet]
    [Route("/voidOk")]
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
    [Route("/withExceptionsNoType")]
    public Results<Ok<TypedResultExample>, Forbidden, InternalServerError> TypedResultWithExceptionsNoType(int id)
    {
        return id switch
        {
            0 => TypedResultsExtensions.Forbidden(),
            < 0 => TypedResultsExtensions.InternalServerError(),
            _ => TypedResults.Ok(new TypedResultExample("Example"))
        };
    }

    [HttpGet]
    [Route("/withExceptionsWithType")]
    public Results<Ok<TypedResultExample>, Forbidden<string>, InternalServerError<string>> TypedResultWithExceptionsWithType(int id)
    {
        return id switch
        {
            0 => TypedResultsExtensions.Forbidden("Forbidden"),
            < 0 => TypedResultsExtensions.InternalServerError("An error occured when processing the request."),
            _ => TypedResults.Ok(new TypedResultExample("Example"))
        };
    }

    [HttpGet]
    [Route("/validateOkNotPresent")]
    public Results<Created<TypedResultExample>, Forbidden<string>, InternalServerError<string>> TypedResultWithOutOk(int id)
    {
        return id switch
        {
            0 => TypedResultsExtensions.Forbidden("Forbidden"),
            < 0 => TypedResultsExtensions.InternalServerError("An error occured when processing the request."),
            _ => TypedResults.Created("hardcoded uri", new TypedResultExample("Example"))
        };
    }

    [HttpGet]
    [Route("/validateOkNotPresentButAnnotationPresent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(string))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public Results<Created<TypedResultExample>, Forbidden<string>, InternalServerError<string>> TypedResultWithoutOkButAnnotationPresent(int id)
    {
        return id switch
        {
            0 => TypedResultsExtensions.Forbidden("Forbidden"),
            < 0 => TypedResultsExtensions.InternalServerError("An error occured when processing the request."),
            _ => TypedResults.Created("hardcoded uri", new TypedResultExample("Example"))
        };
    }
}