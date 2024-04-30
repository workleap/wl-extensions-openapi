﻿using Workleap.Extensions.OpenAPI.Analyzers.Tests;

public class CompareTypedResultWithAnnotationAnalyzerTests : BaseAnalyzerTest<CompareTypedResultWithAnnotationAnalyzer>
{
    [Fact]
    public async Task Given_NoAnnotationIActionResult_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
[ApiController]
[Route("Analyzers")]
public class AnalyzersController : ControllerBase
{
  [HttpGet]
  public IActionResult GetExplicitOperationIdInName() => throw null;
}
""";

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ProducesAndIActionResult_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                [HttpGet]
                                [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                public IActionResult GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_NoAnnotationTypedResult_When_Analyze_Then_No_Diagnostic()
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
    
    [Fact]
    public async Task Given_ProducesAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                 [HttpGet]
                                 [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                 [ProducesResponseType(StatusCodes.Status404NotFound)]
                                 public Results<Ok<String>, NotFound> GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_SwaggerResponseAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
[ApiController]
[Route("Analyzers")]
public class AnalyzersController : ControllerBase
{
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns string", typeof(string), "application/json")]
    public Ok<string> GetExplicitOperationIdInName() => throw null;
}
""";

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ProducesResponseAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
[ApiController]
[Route("Analyzers")]
public class AnalyzersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public Ok<string> GetExplicitOperationIdInName() => throw null;
}
""";

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ProducesResponseAndCorrectTypedResultsTask_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status404NotFound)]
                                  public async Task<Results<Ok<String>, NotFound>> GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ExtraProducesResponseAndCorrectTypedResults_When_Analyze_Then_No_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status404NotFound)]
                                  public Ok<string> GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ProducesResponseAndMismatchTypedResults_When_Analyze_Then_Diagnostic()
    {
        const string source = """
[ApiController]
[Route("Analyzers")]
public class AnalyzersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public Ok<int> {|MyFirstAnalyzer:GetExplicitOperationIdInName|}() => throw null;
}
""";

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_DuplicateProducesResponse_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
                                  public Ok<int> {|MyFirstAnalyzer:GetExplicitOperationIdInName|}() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ProducesResponseTypedAndMismatchTypedResults_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType<string>(StatusCodes.Status200OK)]
                                  public {|MyFirstAnalyzer:Ok<int>|} GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_SwaggerResponseAndMismatchTypedResults_When_Analyze_Then_Diagnostic()
    {
        const string source = """
[ApiController]
[Route("Analyzers")]
public class AnalyzersController : ControllerBase
{
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns TypedResult", typeof(string))]
    public {|MyFirstAnalyzer:Ok<int>|} GetExplicitOperationIdInName() => throw null;
}
""";

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ProducesResponsesAndMismatchTypedResultsTask_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status404NotFound)]
                                  public async Task<Results<Ok<int>, NotFound>> GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    [Fact]
    public async Task Given_ProducesResponsesAndTwoMismatchTypedResultsTask_When_Analyze_Then_Diagnostic()
    {
        const string source = """
                              [ApiController]
                              [Route("Analyzers")]
                              public class AnalyzersController : ControllerBase
                              {
                                  [HttpGet]
                                  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
                                  [ProducesResponseType(StatusCodes.Status404NotFound)]
                                  public async Task<Results<Ok<int>, NotFound<int>>> GetExplicitOperationIdInName() => throw null;
                              }
                              """;

        await this.WithSourceCode(source)
            .RunAsync();
    }
    
    // [Fact]
    // public async Task Given_ProducesResponseAndCorrectTypedResultsTaskWithInternalServerError_When_Analyze_Then_No_Diagnostic()
    // {
    //     const string source = """
    //                           [ApiController]
    //                           [Route("Analyzers")]
    //                           public class AnalyzersController : ControllerBase
    //                           {
    //                               [HttpGet]
    //                               [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    //                               [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //                               public async Task<Results<Ok<string>, InternalServerError>> GetExplicitOperationIdInName() => throw null;
    //                           }
    //                           """;
    //
    //     await this.WithSourceCode(source)
    //         .RunAsync();
    // }
}