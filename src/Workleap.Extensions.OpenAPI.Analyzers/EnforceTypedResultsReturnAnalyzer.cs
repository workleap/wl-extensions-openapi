using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Workleap.Extensions.OpenAPI.Analyzers.Internals;

namespace Workleap.Extensions.OpenAPI.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnforceTypedResultsReturnAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor EndpointReturnTypedResult = new(
        id: RuleIdentifiers.HasTypedResultsUsage,
        title: "Validate endpoint return type",
        messageFormat: "Validate endpoint return type",
        category: RuleCategories.Usage,
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
        private INamedTypeSymbol? ResultSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.IResult");
        private INamedTypeSymbol? ActionResultSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.IActionResult");

        public bool IsValid => this.ActionResultSymbol is not null && this.ResultSymbol is not null;

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
                return this.UnwrapTypeFromTask(typedReturnType);
            }

            return null;
        }

        private INamedTypeSymbol UnwrapTypeFromTask(INamedTypeSymbol typedReturnType)
        {
            // Check if the return type is of Task<> or ValueOfTask<>. If yes, then keep the inner type.
            if (!SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, this.TaskOfTSymbol) &&
                !SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, this.ValueTaskOfTSymbol))
            {
                return typedReturnType;
            }

            var subType = typedReturnType.TypeArguments[0];
            if (subType is INamedTypeSymbol namedSubType)
            {
                typedReturnType = namedSubType;
            }

            return typedReturnType;
        }

        private bool IsWeaklyTypedResults(ITypeSymbol currentClassSymbol)
        {
            return SymbolEqualityComparer.Default.Equals(currentClassSymbol, this.ResultSymbol) || SymbolEqualityComparer.Default.Equals(currentClassSymbol, this.ActionResultSymbol);
        }
    }
}