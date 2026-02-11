using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
#if !NET10_0_OR_GREATER
using Workleap.Extensions.OpenAPI.TypedResult;
#endif

namespace WebApi.OpenAPI.SystemTest.Ordering;

// Repro case of a bug where a ProducesResponseType on the controller level messes up the ordering of the responses
[ApiController]
[Route("ordering")]
[ProducesResponseType(typeof(IEnumerable<ForbiddenReason>), 403, MediaTypeNames.Application.Json)]
public class OrderingController : ControllerBase
{
    [HttpPost("withLotsOfResults")]
#if NET10_0_OR_GREATER
    public async Task<Results<Ok<string>, NotFound, BadRequest, Workleap.Extensions.OpenAPI.TypedResult.InternalServerError<ProblemDetails>>> WithLotsOfResults()
#else
    public async Task<Results<Ok<string>, NotFound, BadRequest, InternalServerError<ProblemDetails>>> WithLotsOfResults()
#endif
    {
        await Task.CompletedTask;
        return TypedResults.Ok("Result");
    }

    private sealed record ForbiddenReason(string Reason);
}
