using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public class TypedResultNoProducesController
{
    [HttpGet]
    [Route("/useApplicationJsonContentType")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public Ok<string> GivenTypedResultAndNoContenTypeThenContentTypeApplicationJson()
    {
        return TypedResults.Ok("example");
    }

    [HttpGet]
    [Route("/usetextplainContentType")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    public Ok<string> GivenTypedResultAndContentTypeThenKeepContentType()
    {
        return TypedResults.Ok("example");
    }
}