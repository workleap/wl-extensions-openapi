using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.OperationId;

[ApiController]
[Route("RequiredType")]
public class RequiredTypeController : ControllerBase
{
    [HttpGet("/recordClassRequiredType", Name = "GetRecordClassRequiredType")]
    [ProducesResponseType(typeof(RequiredExample), StatusCodes.Status200OK)]
    public IActionResult RecordClassRequiredType()
    {
        return this.Ok(new RequiredExample(0, "example", null, 0));
    }
}

public sealed record RequiredExample(int RequiredIntProperty, string RequiredStringProperty, string? OptionalStringProperty, int? OptionalIntProperty);