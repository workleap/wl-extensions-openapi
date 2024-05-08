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

    [HttpGet("/classRequiredType", Name = "GetClassRequiredType")]
    [ProducesResponseType(typeof(RequiredExampleGrandParentClass), StatusCodes.Status200OK)]
    public IActionResult ClassRequiredType()
    {
        return this.Ok();
    }
}

public sealed record ParentRequiredExample(string Id)
{
    public sealed record RequiredExample(IReadOnlyCollection<RequiredExample.Example> Examples, string? OptionalStringProperty)
    {
        public sealed record Example(IReadOnlyCollection<Example.Result> Results, string Property)
        {
            public sealed record Result(string Id, string? OptionalProperty);
        }
    }
}

public sealed class RequiredExampleGrandParentClass(RequiredExampleParentClass requiredExampleParent, string? optionalStringProperty)
{
    public RequiredExampleParentClass RequiredExampleParent { get; } = requiredExampleParent;
    public string? OptionalStringProperty { get; } = optionalStringProperty;
}

public sealed class RequiredExampleParentClass(IReadOnlyCollection<RequiredExampleClass> requiredExamples, string? optionalStringProperty)
{
    public IReadOnlyCollection<RequiredExampleClass> Examples { get; } = requiredExamples;
    public string? OptionalStringProperty { get; } = optionalStringProperty;
}

public sealed class RequiredExampleClass(IReadOnlyCollection<ResultExampleClass> resultExamples, string property)
{
    public IReadOnlyCollection<ResultExampleClass> Results { get; } = resultExamples;
    public string Property { get; } = property;
}

public sealed class ResultExampleClass(string id, string? optionalProperty)
{
    public string Id { get; } = id;
    public string? OptionalProperty { get; } = optionalProperty;
}