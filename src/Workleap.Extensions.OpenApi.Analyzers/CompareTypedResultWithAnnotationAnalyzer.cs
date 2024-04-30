using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CompareTypedResultWithAnnotationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MyFirstAnalyzer";

    private static readonly LocalizableString Title = "Title of the issue";
    private static readonly LocalizableString MessageFormat = "Message format for the issue";
    private static readonly LocalizableString Description = "Description of the issue";
    private const string Category = "Naming";

    public static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeClassDeclaration, SymbolKind.Method);
    }

    private void AnalyzeClassDeclaration(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        var returnType =methodSymbol.ReturnType;

        if (returnType is INamedTypeSymbol typedReturnType)
        {
            // typedReturnType.Const
        }

        var b = methodSymbol.GetAttributes();
    }
}