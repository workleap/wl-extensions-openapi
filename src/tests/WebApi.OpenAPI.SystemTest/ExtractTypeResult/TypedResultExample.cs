namespace WebApi.OpenAPI.SystemTest.ExtractTypeResult;

public class TypedResultExample
{
    public TypedResultExample(string name)
    {
        this.Name = name;
    }

    public string Name { get; set; }
    
    public int Count { get; set; }
    
    public string? Description { get; set; }
}