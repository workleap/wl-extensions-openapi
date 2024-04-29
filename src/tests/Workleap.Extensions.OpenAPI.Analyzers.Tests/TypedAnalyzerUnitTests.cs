using Workleap.Extensions.OpenAPI.Analyzers.Tests;

public class TypedAnalyzerUnitTests : BaseAnalyzerTest<TrueAnalyzers>
{

    [Fact]
    public async Task AnalyzerFindsBadIdentifier2()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  public Ok<string> GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
}