using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public class TypedResultProperContentTypeController
{
    [HttpGet]
    [EndpointName("OkNoContentType")]
    [Route("/useApplicationJsonContentTypeWithOk")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public Ok GivenOkTypedResultAndNoContenTypeThenContentTypeApplicationJson()
    {
        return TypedResults.Ok();
    }

    [HttpGet]
    [EndpointName("TemplatedOkNoContentType")]
    [Route("/useApplicationJsonContentTypeWithOk<T>")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public Ok<string> GivenTemplatedOkTypedResultAndNoContenTypeThenContentTypeApplicationJson()
    {
        return TypedResults.Ok("example");
    }

    // [HttpGet]
    // [EndpointName("NoContentType")]
    // [Route("/useApplicationJsonContentTypeWithNoContent")]
    // public NoContent GivenNoContentTypeResultThenNoContentTypeApplicationJson()
    // {
    //     return TypedResults.NoContent();
    // }

    [HttpGet]
    [EndpointName("ResultsNoContentType")]
    [Route("/useApplicationJsonContentTypeWithResultsType")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound, "text/plain")]
    public Results<Ok<string>, BadRequest<string>, NotFound<string>> GivenResultsTypeAndNoContentTypeThenContentTypeApplicationJson()
    {
        return TypedResults.Ok("example");
    }

    [HttpGet]
    [EndpointName("OkContentTypeTextPlain")]
    [Route("/overwriteContenTypeWithProduceAttributeTextPlainForOk")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    public Ok GivenOkTypedResultAndContentTypeThenKeepContentType()
    {
        return TypedResults.Ok();
    }

    [HttpGet]
    [EndpointName("TemplatedOkContentTypeTextPlain")]
    [Route("/overwriteContenTypeWithProduceAttributeTextPlainForOk<T>")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    public Ok<string> GivenTemplatedOkTypedResultAndContentTypeThenKeepContentType()
    {
        return TypedResults.Ok("example");
    }

    [HttpGet]
    [EndpointName("ResultsContentTypeTextPlain")]
    [Route("/overwriteContenTypeWithProduceAttributeTextPlainForResultsType")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound, "text/plain")]
    public Results<Ok<string>, BadRequest<string>, NotFound<string>> GivenResultsTypeAndContentTypeThenKeepContentType()
    {
        return TypedResults.Ok("example");
    }
}