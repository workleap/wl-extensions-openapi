namespace Workleap.Extensions.OpenAPI.Builder;

public sealed class JsonSerializerDifferenceException(string propertyName) : Exception($"JsonSerializerOptions for {propertyName} is different.");