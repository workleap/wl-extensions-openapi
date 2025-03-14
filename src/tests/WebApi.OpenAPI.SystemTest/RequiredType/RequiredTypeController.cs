using Microsoft.AspNetCore.Mvc;

namespace WebApi.OpenAPI.SystemTest.RequiredType;

[ApiController]
[Produces("application/json")]
public class RequiredTypeController : ControllerBase
{
    [HttpGet("/recordClassRequiredType", Name = "GetRecordClassRequiredType")]
    [ProducesResponseType(typeof(RequiredExampleGrandParentRecord), StatusCodes.Status200OK)]
    public IActionResult RecordClassRequiredType()
    {
        return this.Ok();
    }

    [HttpGet("/classRequiredType", Name = "GetClassRequiredType")]
    [ProducesResponseType(typeof(RequiredExampleGrandParentClass), StatusCodes.Status200OK)]
    public IActionResult ClassRequiredType()
    {
        return this.Ok();
    }
}

public sealed record RequiredExampleGrandParentRecord(RequiredExampleGrandParentRecord.RequiredExampleParentRecord RequiredExampleParent, string? OptionalGrandParentProperty)
{
    public sealed record RequiredExampleParentRecord(IReadOnlyCollection<RequiredExampleParentRecord.RequiredExampleRecord> RequiredExamples, string? OptionalParentProperty)
    {
        public sealed record RequiredExampleRecord(IReadOnlyCollection<RequiredExampleRecord.ResultRecord> Results, string ExampleProperty)
        {
            public sealed record ResultRecord(string ResultId, string? OptionalResultProperty);
        }
    }
}

public sealed class RequiredExampleGrandParentClass(RequiredExampleParentClass requiredExampleParent, string? optionalGrandParentProperty)
{
    public RequiredExampleParentClass RequiredExampleParent { get; } = requiredExampleParent;
    public string? OptionalGrandParentProperty { get; } = optionalGrandParentProperty;
}

public sealed class RequiredExampleParentClass(IReadOnlyCollection<RequiredExampleClass> requiredExamples, string? optionalParentProperty)
{
    public IReadOnlyCollection<RequiredExampleClass> RequiredExamples { get; } = requiredExamples;
    public string? OptionalParentProperty { get; } = optionalParentProperty;
}

public sealed class RequiredExampleClass(IReadOnlyCollection<ResultClass> results, string exampleProperty)
{
    public IReadOnlyCollection<ResultClass> Results { get; } = results;
    public string ExampleProperty { get; } = exampleProperty;
}

public sealed class ResultClass(string resultId, string? optionalResultProperty)
{
    public string ResultId { get; } = resultId;
    public string? OptionalResultProperty { get; } = optionalResultProperty;
}