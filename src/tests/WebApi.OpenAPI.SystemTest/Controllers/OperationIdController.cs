using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.OpenAPI.SystemTest.Properties;

[ApiController]
[Route("OperationId")]
public class OperationIdController: ControllerBase
{
    [HttpGet("/explicitOperationIdInName", Name = "GetExplicitOperationIdInName")]
    public IActionResult GetExplicitOperationIdInName()
    {
        return this.Ok();
    }

    [HttpGet(Name = "GetExplicitOperationIdInNameWithRoute")]
    [Route("/explicitOperationIdInSwagger")]
    public IActionResult GetExplicitOperationIdInNameWithRouteAttribute()
    {
        return this.Ok();
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