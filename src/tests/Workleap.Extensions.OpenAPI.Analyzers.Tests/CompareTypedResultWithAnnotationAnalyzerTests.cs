using Workleap.Extensions.OpenAPI.Analyzer;

namespace Workleap.Extensions.OpenAPI.Analyzers.Tests;

public class CompareTypedResultWithAnnotationAnalyzerTests : BaseAnalyzerTest<CompareTypedResultWithAnnotationAnalyzer>
{
    [Fact]
    public async Task Given_NoAnnotationIActionResult_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                public IActionResult GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesAndIActionResult_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                public IActionResult GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_NoAnnotationTypedResult_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  public Ok<string> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                 [HttpGet]
                                 [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                 [ProducesResponseType(StatusCodes.Status404NotFound)]
                                 public Results<Ok<String>, NotFound> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_SwaggerResponseAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [SwaggerResponse(StatusCodes.Status200OK, "Returns string", typeof(string), "application/json")]
                                  public Ok<string> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponseAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  public Ok<string> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponseAndCorrectTypedResultsTask_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status404NotFound)]
                                  public async Task<Results<Ok<String>, NotFound>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ExtraProducesResponseAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status404NotFound)]
                                  public Ok<string> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponseAndMismatchTypedResults_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:ProducesResponseType(typeof(string), StatusCodes.Status200OK)|}]
                                  public Ok<int> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_DuplicateProducesResponse_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:ProducesResponseType(typeof(string), StatusCodes.Status200OK)|}]
                                  [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
                                  public Ok<int> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponseTypedAndMismatchTypedResults_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:ProducesResponseType<string>(StatusCodes.Status200OK)|}]
                                  public Ok<int> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_SwaggerResponseAndMismatchTypedResults_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:SwaggerResponse(StatusCodes.Status200OK, "Returns TypedResult", typeof(string))|}]
                                  public Ok<int> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponsesAndMismatchTypedResultsTask_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:ProducesResponseType(typeof(string), StatusCodes.Status200OK)|}]
                                  [ProducesResponseType(StatusCodes.Status404NotFound)]
                                  public async Task<Results<Ok<int>, NotFound>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponsesAndTwoMismatchTypedResultsTask_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:ProducesResponseType(typeof(string), StatusCodes.Status200OK)|}]
                                  [{|WLOAS001:ProducesResponseType(StatusCodes.Status404NotFound)|}]
                                  public async Task<Results<Ok<int>, NotFound<int>>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponseAndCorrectTypedResultsTaskWithInternalServerError_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
                                  public async Task<Results<Ok<string>, InternalServerError>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponseAndCorrectTypedResultsTaskWithForbidden_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status403Forbidden)]
                                  public async Task<Results<Ok<string>, Forbidden>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
}