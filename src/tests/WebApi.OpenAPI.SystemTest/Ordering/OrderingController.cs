using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace WebApi.OpenAPI.SystemTest.Ordering;

// Repro case of a bug where a ProducesResponseType on the controller level messes up the ordering of the responses
[ApiController]
[Route("ordering")]
[ProducesResponseType(typeof(IEnumerable<ForbiddenReason>), 403, MediaTypeNames.Application.Json)]
public class OrderingController : ControllerBase
{
    [HttpPost("withLotsOfResults")]
    public async Task<Results<Ok<string>, NotFound, BadRequest, InternalServerError<ProblemDetails>>> WithLotsOfResults()
    {
        await Task.CompletedTask;
        return TypedResults.Ok("Result");
    }

    private sealed record ForbiddenReason(string Reason);
}
