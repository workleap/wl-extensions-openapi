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

    [Fact]
    public async Task Given_ProducesResponseNamedArgumentsAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
                                  [ProducesResponseType(statusCode: StatusCodes.Status202Accepted, type: typeof(string))]
                                  [ProducesResponseType(StatusCodes.Status403Forbidden)]
                                  public async Task<Results<Ok<string>, Accepted<string>, Forbidden>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_ProducesResponseNamedArgumentsAndMismatchTypedResults_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))|}]
                                  [{|WLOAS001:ProducesResponseType(statusCode: StatusCodes.Status202Accepted, type: typeof(string))|}]
                                  [{|WLOAS001:ProducesResponseType(StatusCodes.Status403Forbidden)|}]
                                  public async Task<Results<Ok<int>, Accepted, Forbidden<string>>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_SwaggerResponseNamedArgumentsAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(string))]
                                  [SwaggerResponse(statusCode: StatusCodes.Status202Accepted, "Return string", Type = typeof(string))]
                                  [SwaggerResponse(type: typeof(string), statusCode: StatusCodes.Status400BadRequest)]
                                  [SwaggerResponse(StatusCodes.Status403Forbidden, "Returns string", ContentTypes = ["application/json"])]
                                  [SwaggerResponse(StatusCodes.Status500InternalServerError, "Returns string", typeof(string), ContentTypes = ["application/json"])]
                                  public async Task<Results<Ok<string>, Accepted<string>, BadRequest<string>, Forbidden, InternalServerError<string>>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }

    [Fact]
    public async Task Given_SwaggerResponseNamedArgumentsAndMismatchTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [{|WLOAS001:SwaggerResponse(StatusCodes.Status200OK, Type = typeof(string))|}]
                                  [{|WLOAS001:SwaggerResponse(statusCode: StatusCodes.Status202Accepted, "Return string", Type = typeof(string))|}]
                                  [{|WLOAS001:SwaggerResponse(type: typeof(string), statusCode: StatusCodes.Status400BadRequest)|}]
                                  [{|WLOAS001:SwaggerResponse(StatusCodes.Status403Forbidden, "Returns string", ContentTypes = ["application/json"])|}]
                                  [{|WLOAS001:SwaggerResponse(StatusCodes.Status500InternalServerError, "Returns string", typeof(string), ContentTypes = ["application/json"])|}]
                                  public async Task<Results<Ok<int>, Accepted, BadRequest<int>, Forbidden<int>, InternalServerError>> GetSampleEndpoint() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
}