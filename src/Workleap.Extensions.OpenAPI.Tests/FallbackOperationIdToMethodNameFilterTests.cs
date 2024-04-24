namespace Workleap.Extensions.OpenAPI.Tests;

public class FallbackOperationIdToMethodNameFilterTests
{
    [Theory]
    [InlineData("GetData", "GetData")]
    [InlineData("GetDataAsync", "GetData")]
    [InlineData("GetDataasync", "GetData")]
    public void Given_Method_Name_When_Cleanup_Then_Clean_Name(string methodName, string expectedOutput)
    {
        // When
        var result = FallbackOperationIdToMethodNameFilter.CleanupName(methodName);

        // Then
        Assert.Equal(expectedOutput, result);
    }
}