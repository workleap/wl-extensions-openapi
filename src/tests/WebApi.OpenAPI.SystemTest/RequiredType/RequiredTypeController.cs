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
    [ProducesResponseType(typeof(ParentRequiredExampleClass), StatusCodes.Status200OK)]
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

public sealed class ParentRequiredExampleClass(RequiredExampleClass requiredExample, string? optionalStringProperty)
{
    public RequiredExampleClass RequiredExample { get; } = requiredExample;
    public string? OptionalStringProperty { get; } = optionalStringProperty;
}

public sealed class RequiredExampleClass(IReadOnlyCollection<ExampleClass> Examples, string? OptionalStringProperty)
{
    public IReadOnlyCollection<ExampleClass> Examples { get; } = Examples;
    public string? OptionalStringProperty { get; } = OptionalStringProperty;
}

public sealed class ExampleClass(IReadOnlyCollection<ResultClass> Results, string Property)
{
    public IReadOnlyCollection<ResultClass> Results { get; } = Results;
    public string Property { get; } = Property;
}

public sealed class ResultClass(string Id, string? OptionalProperty)
{
    public string Id { get; } = Id;
    public string? OptionalProperty { get; } = OptionalProperty;
}