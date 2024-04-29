using Workleap.Extensions.OpenAPI.Analyzers.Tests;

public class MyFirstAnalyzerUnitTests : BaseAnalyzerTest<MyFirstAnalyzer>
{
    [Fact]
    public async Task AnalyzerFindsBadIdentifier()
    {
        const string source = """
                              public class BadClassName
                              {
                                  public void BadMethodName()
                                  {
                                      var BadVariableName = ""This is a bad variable name"";
                                  }
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
}