namespace WebApi.OpenAPI.SystemTest.OperationId;

public static class OperationIdMinimalApis
{
    public static void AddEndpointsForOperationId(this WebApplication app)
    {
        app.MapGet("minimal-endpoint-with-name", (() => Results.Ok()))
            .WithName("GetMinimalApiWithName")
            .WithTags("OperationId")
            .WithOpenApi();

        app.MapGet("minimal-endpoint-with-no-name", () => Results.Ok())
            .WithTags("OperationId")
            .WithOpenApi();
    }
}
