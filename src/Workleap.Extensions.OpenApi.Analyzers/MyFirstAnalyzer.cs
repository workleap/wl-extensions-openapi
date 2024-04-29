using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MyFirstAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MyFirstAnalyzer";

    private static readonly LocalizableString Title = "Title of the issue";
    private static readonly LocalizableString MessageFormat = "Message format for the issue";
    private static readonly LocalizableString Description = "Description of the issue";
    private const string Category = "Naming";

    private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IdentifierName);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var identifierName = (IdentifierNameSyntax)context.Node;

        // Replace with your own logic
        if (identifierName.Identifier.Text.StartsWith("Bad"))
        {
            var diagnostic = Diagnostic.Create(Rule, identifierName.GetLocation(), identifierName.Identifier.Text);

            context.ReportDiagnostic(diagnostic);
        }
    }
}