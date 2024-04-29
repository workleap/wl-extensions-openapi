using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.OpenAPI.SystemTest.OperationId;

[ApiController]
[Route("OperationId")]
public class OperationIdController : ControllerBase
{
    [HttpGet("/explicitOperationIdInName", Name = "GetExplicitOperationIdInName")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType( typeof(string), StatusCodes.Status200OK)]
    public Ok<string> GetExplicitOperationIdInName()
    {
        return TypedResults.Ok("Hello World");
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetExplicitOperationIdInSwagger")]
    [Route("/explicitOperationIdInSwagger")]
    public IActionResult GetExplicitOperationIdInSwagger()
    {
        return this.Ok();
    }

    [HttpGet]
    [Route("/noOperationId")]
    public IActionResult GetNotOperationId()
    {
        return this.Ok();
    }
}