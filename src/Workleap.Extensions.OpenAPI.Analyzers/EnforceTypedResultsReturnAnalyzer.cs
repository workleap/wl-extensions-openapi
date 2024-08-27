using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Workleap.Extensions.OpenAPI.Analyzers.Internals;

namespace Workleap.Extensions.OpenAPI.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnforceTypedResultsReturnAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor EndpointReturnTypedResult = new(
        id: RuleIdentifiers.HasTypedResultsUsage,
        title: "Enforce strongly typed endpoint response",
        messageFormat: "Enforce strongly typed endpoint response such as Results<> or Ok<>",
        category: RuleCategories.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: RuleIdentifiers.HelpUri);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EndpointReturnTypedResult);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var analyzerContext = new AnalyzerContext(compilationContext.Compilation);
            if (analyzerContext.IsValid)
            {
                compilationContext.RegisterSymbolAction(analyzerContext.ValidateEndpointResponseType, SymbolKind.Method);
            }
        });
    }

    private sealed class AnalyzerContext(Compilation compilation)
    {
        private INamedTypeSymbol? TaskOfTSymbol { get; } = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        private INamedTypeSymbol? ValueTaskOfTSymbol { get; } = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        private INamedTypeSymbol? ResultInterfaceSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.IResult");
        private INamedTypeSymbol? ActionResultInterfaceSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.IActionResult");
        private INamedTypeSymbol? ActionResultSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ActionResult");

        public bool IsValid => this.ActionResultInterfaceSymbol is not null || this.ResultInterfaceSymbol is not null;

        public void ValidateEndpointResponseType(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;
            var typedReturnType = this.GetReturnTypeSymbol(methodSymbol);

            if (typedReturnType == null || !this.IsWeaklyTypedResults(typedReturnType))
            {
                return;
            }

            context.ReportDiagnostic(EndpointReturnTypedResult, context.Symbol);
        }

        private INamedTypeSymbol? GetReturnTypeSymbol(IMethodSymbol methodSymbol)
        {
            var returnType = methodSymbol.ReturnType;

            if (returnType is INamedTypeSymbol typedReturnType)
            {
                return RoslynExtensions.UnwrapTypeFromTask(typedReturnType, this.TaskOfTSymbol, this.ValueTaskOfTSymbol);
            }

            return null;
        }

        private bool IsWeaklyTypedResults(ITypeSymbol currentClassSymbol)
        {
            return SymbolEqualityComparer.Default.Equals(currentClassSymbol, this.ResultInterfaceSymbol)
                   || SymbolEqualityComparer.Default.Equals(currentClassSymbol, this.ActionResultInterfaceSymbol)
                   || SymbolEqualityComparer.Default.Equals(currentClassSymbol, this.ActionResultSymbol);
        }
    }
}