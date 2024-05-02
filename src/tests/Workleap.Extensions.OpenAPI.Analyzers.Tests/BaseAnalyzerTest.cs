using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Workleap.Extensions.OpenAPI.TypedResult;

namespace Workleap.Extensions.OpenAPI.Analyzers.Tests;

public class BaseAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private const string GlobalUsingList = """
                                           global using System;
                                           global using System.Collections.Generic;
                                           global using System.IO;
                                           global using System.Linq;
                                           global using System.Threading;
                                           global using System.Threading.Tasks;
                                           global using Microsoft.AspNetCore.Http.HttpResults;
                                           global using Microsoft.AspNetCore.Http;
                                           global using Microsoft.AspNetCore.Mvc;
                                           global using Swashbuckle.AspNetCore.Annotations;
                                           global using Workleap.Extensions.OpenAPI.TypedResult;
                                           """;

    private const string SourceFileName = "Program.cs";

    protected BaseAnalyzerTest()
    {
        this.TestState.Sources.Add(GlobalUsingList);
        this.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;

        this.TestState.ReferenceAssemblies = this.TestState.ReferenceAssemblies.AddPackages(
            ImmutableArray.Create(
                new PackageIdentity("Microsoft.AspNetCore.App.Ref", "8.0.4"),
                new PackageIdentity("Swashbuckle.AspNetCore.Annotations", "6.5.0")
            ));

        this.TestState.AdditionalReferences.Add(typeof(InternalServerError).Assembly);
    }

    // Specify compilation options as DLL and that unsafe code is not allowed.
    protected override CompilationOptions CreateCompilationOptions()
    {
        return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: false);
    }

    // Specify the language version and that diagnostics should be reported for missing documentation.
    protected override ParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(LanguageVersion.CSharp11, DocumentationMode.Diagnose);
    }

    protected BaseAnalyzerTest<TAnalyzer> WithSourceCode(string sourceCode)
    {
        this.TestState.Sources.Add((SourceFileName, sourceCode));
        return this;
    }
}