using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.Analyzers;


[ApiController]
[Route("Analyzers")]
public class AnalyzersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetExplicitOperationIdInName()
    {
        var badNaming = 1;
        return this.Ok(badNaming);
    }
}

public class BadClassName
{
    public void BadMethodName()
    {
        var BadVariableName = "This is a bad variable name";
    }
}