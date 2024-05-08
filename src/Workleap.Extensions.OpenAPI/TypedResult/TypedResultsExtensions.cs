namespace Workleap.Extensions.OpenAPI.TypedResult;

public static class TypedResultsExtensions
{
    public static InternalServerError InternalServerError() => new();
    public static InternalServerError<T> InternalServerError<T>(T? error) => new(error);

    public static Forbidden Forbidden() => new();
    public static Forbidden<T> Forbidden<T>(T? error) => new(error);
}