namespace Workleap.Extensions.OpenAPI.TypedResult;

public static class TypedResultsExtensions
{
    public static Forbidden Forbidden() => new();
    public static Forbidden<T> Forbidden<T>(T? error) => new(error);
}