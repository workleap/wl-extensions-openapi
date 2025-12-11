using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Workleap.Extensions.OpenAPI.TypedResult;

namespace WebApi.OpenAPI.SystemTest.Ordering;

// test ff ffffff

[ApiController]
[Route("ordering")]
[ProducesResponseType(typeof(IEnumerable<ForbiddenReason>), 403, MediaTypeNames.Application.Json)]
public class OrderingController : ControllerBase
{
    /// <summary>
    /// Test endpoint with ProducesResponseType at class level and multiple TypedResults
    /// This tests non-deterministic ordering of swagger results
    /// </summary>
    [HttpPost("withLostOfResults")]
    public async Task<Results<Ok<string>, NotFound, BadRequest, InternalServerError<ProblemDetails>>> WithLotsOfResults()
    {
        await Task.CompletedTask;
        return TypedResults.Ok("Result");
    }

    private sealed record ForbiddenReason(string Reason);
}
