using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.OperationId;

[ApiController]
[Route("RequiredType")]
[Produces("application/json")]
public class RequiredTypeController : ControllerBase
{
    [HttpGet("/recordClassRequiredType", Name = "GetRecordClassRequiredType")]
    [ProducesResponseType(typeof(ParentRequiredExample.RequiredExample), StatusCodes.Status200OK)]
    public IActionResult RecordClassRequiredType()
    {
        return this.Ok();
    }
}

public sealed record ParentRequiredExample(string Id)
{
    public sealed record RequiredExample(IReadOnlyCollection<RequiredExample.Example> Examples, string? OptionalStringProperty)
    {
        public sealed record Example(string RequiredId);
    }
}
