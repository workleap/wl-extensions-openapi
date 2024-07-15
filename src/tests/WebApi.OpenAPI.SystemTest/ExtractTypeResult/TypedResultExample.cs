namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public class TypedResultExample
{
    public TypedResultExample(string name)
    {
        this.Name = name;
    }

    public TypedResultExample(string name, OperationEnum operation)
    {
        this.Name = name;
        this.Operation = operation;
    }

    public string Name { get; set; }

    public int Count { get; set; }

    public string? Description { get; set; }

    public OperationEnum Operation { get; set; }
}

public enum OperationEnum
{
    Foo,
    Bar,
    Foobar
}