using Workleap.Extensions.OpenAPI.OperationId;

namespace Workleap.Extensions.OpenAPI.Tests.OperationId;

public class FallbackOperationIdToMethodNameFilterTests
{
    [Theory]
    [InlineData("GetData", "GetData")]
    [InlineData("GetDataasync", "GetData")]
    [InlineData("GetAsyncDataasync", "GetAsyncData")]
    public void Given_Method_Name_When_Cleanup_Then_Clean_Name(string methodName, string expectedOutput)
    {
        // When
        var result = FallbackOperationIdToMethodNameFilter.GenerateOperationIdFromMethodName(methodName);

        // Then
        Assert.Equal(expectedOutput, result);
    }
}