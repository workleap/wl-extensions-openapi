using Workleap.Extensions.OpenAPI.Analyzer;

namespace Workleap.Extensions.OpenAPI.Analyzers.Tests;

public class EnforceTypedResultsReturnAnalyzerTests : BaseAnalyzerTest<EnforceTypedResultsReturnAnalyzer>
{
    [Fact]
    public async Task Given_ReturnTypeResult_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public Ok GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnTaskOfTypeResultOfT_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public Task<Ok<string>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnSpecificType_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public string GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnTypeResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public Results<Ok<string>, NotFound> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnTaskOfTypeResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public Task<Results<Ok<string>, NotFound>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnActionResultOfT_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public ActionResult<string> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnTaskOfActionResultOfT_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public Task<ActionResult<string>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnIActionResult_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public {|WLOAS002:IActionResult|} GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnTaskOfIActionResult_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public {|WLOAS002:Task<IActionResult>|} GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnIResult_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public {|WLOAS002:IResult|} GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ReturnTaskOfIResult_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public {|WLOAS002:Task<IResult>|} GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
}