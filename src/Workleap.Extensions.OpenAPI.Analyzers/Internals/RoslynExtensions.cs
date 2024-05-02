using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Workleap.Extensions.OpenAPI.Analyzers.Internals;

internal static class RoslynExtensions
{
    public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor diagnosticDescriptor, Location location)
    {
        context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, location));
    }

    public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor diagnosticDescriptor, ISymbol symbol)
    {
        foreach (var location in symbol.Locations)
        {
            context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, location));
        }
    }

    public static INamedTypeSymbol UnwrapTypeFromTask(INamedTypeSymbol typedReturnType, ITypeSymbol? taskOfTSymbol, ITypeSymbol? valueTaskOfTSymbol)
    {
        // Check if the return type is of Task<> or ValueOfTask<>. If yes, then keep the inner type.
        if (SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, taskOfTSymbol) ||
            SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, valueTaskOfTSymbol))
        {
            var subType = typedReturnType.TypeArguments[0];
            if (subType is INamedTypeSymbol namedSubType)
            {
                typedReturnType = namedSubType;
            }
        }

        return typedReturnType;
    }
}