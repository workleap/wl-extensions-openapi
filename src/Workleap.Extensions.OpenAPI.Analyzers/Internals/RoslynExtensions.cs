using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Workleap.Extensions.OpenAPI.Analyzers.Internals;

internal static class RoslynExtensions
{
    public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor diagnosticDescriptor, Location location)
    {
        context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, location));
    }
}