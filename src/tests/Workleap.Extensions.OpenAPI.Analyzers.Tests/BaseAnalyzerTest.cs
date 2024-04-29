using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Workleap.Extensions.OpenAPI.Analyzers.Tests;

public class BaseAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private const string CSharp10GlobalUsings = """
                                                global using System;
                                                global using System.Collections.Generic;
                                                global using System.IO;
                                                global using System.Linq;
                                                global using System.Threading;
                                                global using System.Threading.Tasks;
                                                """;

    private const string SourceFileName = "Program.cs";

    public BaseAnalyzerTest()
    {
        this.TestState.Sources.Add(CSharp10GlobalUsings);
    }

    // TODO: Why this
    protected override CompilationOptions CreateCompilationOptions()
    {
        return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: false);
    }

    // TODO: Why this
    protected override ParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(LanguageVersion.CSharp12, DocumentationMode.Diagnose);
    }

    // TODO: Understand this
    public BaseAnalyzerTest<TAnalyzer> WithExpectedDiagnostic(DiagnosticDescriptor descriptor, int startLine, int startColumn, int endLine, int endColumn, params object[] args)
    {
        this.TestState.ExpectedDiagnostics.Add(new DiagnosticResult(descriptor)
            .WithSpan(SourceFileName, startLine, startColumn, endLine, endColumn)
            .WithArguments(args));
        return this;
    }

    // TODO: Understand this
    public BaseAnalyzerTest<TAnalyzer> WithSourceCode(string sourceCode)
    {
        this.TestState.Sources.Add((SourceFileName, sourceCode));
        return this;
    }
}