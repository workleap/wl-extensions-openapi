using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.Analyzers;


[ApiController]
[Route("Analyzers")]
public class AnalyzersController : ControllerBase
{
    [HttpGet]
    public Ok<string> GetExplicitOperationIdInName()
    {
        return TypedResults.Ok("Hello World!");
    }
}
