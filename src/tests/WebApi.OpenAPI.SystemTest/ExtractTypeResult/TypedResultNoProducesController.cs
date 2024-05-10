using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public class TypedResultNoProducesController
{
    [HttpGet]
    [Route("/useApplicationJsonContentType")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    public Ok<string> TypedResultUseApplicationJsonContentType()
    {
        return TypedResults.Ok("example");
    }
}