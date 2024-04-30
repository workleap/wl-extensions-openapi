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
                                      var BadVariableName = "This is a bad variable name";
                                  }
                              }
                              """;

        await this.WithSourceCode(source)
            .WithExpectedDiagnostic(MyFirstAnalyzer.Rule, 1, 14, 1, 26, "BadClassName")
            // .WithExpectedDiagnostic(MyFirstAnalyzer.Rule, 3, 17, 3, 30, "BadMethodName")
            // .WithExpectedDiagnostic(MyFirstAnalyzer.Rule, 5, 13, 5, 28, "BadVariableName")
            .RunAsync();
    }

    [Fact]
    public async Task AnalyzerFindsBadIdentifier2()
    {
        const string source = """
public class [|BadClassName|]
{
  public void BadMethodName()
  {
      var BadVariableName = "This is a asd variable name";
  }
}
""";

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task AnalyzerFindsBadIdentifier3()
    {
        const string source = """
public class {|MyFirstAnalyzer:BadClassName|}
{
    public void {|MyFirstAnalyzer:BadMethodName|}()
    {
        var BadVariableName = "This is a asd variable name";
    }
}
""";

        await this.WithSourceCode(source)
            .RunAsync();
    }
}