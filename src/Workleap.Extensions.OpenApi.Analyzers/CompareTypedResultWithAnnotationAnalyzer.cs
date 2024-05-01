using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Workleap.Extensions.OpenApi.Analyzers.Internals;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CompareTypedResultWithAnnotationAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "WLOAS001";

    private static readonly LocalizableString Title = "Title of the issue";
    private static readonly LocalizableString MessageFormat = "Message format for the issue";
    private static readonly LocalizableString Description = "Mismatch between annotation and method return type.";
    private const string Category = "Naming";

    private const string ErrorReason = "Mismatch between annotation and method return type.";

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

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var analyzerContext = new AnalyzerContext(compilationContext.Compilation);

            compilationContext.RegisterSymbolAction(analyzerContext.AnalyzeClassDeclaration, SymbolKind.Method);
        });
    }

    private sealed class AnalyzerContext(Compilation compilation)
    {
        private INamedTypeSymbol? TaskOfTSymbol { get; } = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.Task`1");
        private INamedTypeSymbol? ValueTaskOfTSymbol { get; } = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        private INamedTypeSymbol? ProducesResponseSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute");
        private INamedTypeSymbol? ProducesResponseOfTSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute`1");
        private INamedTypeSymbol? SwaggerResponseSymbol { get; } = compilation.GetTypeByMetadataName("Swashbuckle.AspNetCore.Annotations.SwaggerResponseAttribute");
        
        public INamedTypeSymbol?[] ResultTaskOfTSymbol { get; } =
        [
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`2"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`3"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`4"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`5"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`6"),
        ];


        private readonly Dictionary<ITypeSymbol, int> _resultsToStatusCodeMap = InitializeHttpResultStatusCodeMap(compilation);
        private readonly Dictionary<int, ITypeSymbol> _statusCodeToResultsMap = InitializeStatusCodeMapHttpResultMap(compilation);

        private static Dictionary<ITypeSymbol, int> InitializeHttpResultStatusCodeMap(Compilation compilation)
        {
            // TODO initialize with other response types
            var dictionary = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
            Add("Microsoft.AspNetCore.Http.HttpResults.Ok", 200);
            Add("Microsoft.AspNetCore.Http.HttpResults.Ok`1", 200);
            Add("Microsoft.AspNetCore.Http.HttpResults.NotFound", 404);
            Add("Microsoft.AspNetCore.Http.HttpResults.NotFound`1", 404);
            // Supported in .NET 9
            Add("Microsoft.AspNetCore.Http.HttpResults.InternalServerError", 500);
            Add("Microsoft.AspNetCore.Http.HttpResults.InternalServerError`1", 500);
            Add("Workleap.Extensions.OpenAPI.TypedResult.InternalServerError", 500);
            Add("Workleap.Extensions.OpenAPI.TypedResult.InternalServerError`1", 500);

            return dictionary;

            void Add(string metadata, int statusCode)
            {
                var type = compilation.GetTypeByMetadataName(metadata);
                if (type is not null)
                {
                    dictionary.Add(type, statusCode);
                }
            }
        }

        private static Dictionary<int, ITypeSymbol> InitializeStatusCodeMapHttpResultMap(Compilation compilation)
        {
            // TODO initialize with other response types
            var dictionary = new Dictionary<int, ITypeSymbol>();
            Add(200, "Microsoft.AspNetCore.Http.HttpResults.Ok");
            Add(404, "Microsoft.AspNetCore.Http.HttpResults.NotFound");
            Add(500, "Microsoft.AspNetCore.Http.HttpResults.InternalServerError");
            Add(500, "Workleap.Extensions.OpenAPI.TypedResult.InternalServerError");

            return dictionary;

            void Add(int statusCode, string metadata)
            {
                var type = compilation.GetTypeByMetadataName(metadata);
                if (type is not null)
                {
                    dictionary.Add(statusCode, type);
                }
            }
        }

        public void AnalyzeClassDeclaration(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;
            var returnType = methodSymbol.ReturnType;

            if (returnType is INamedTypeSymbol typedReturnType)
            {
                var iResultTypeSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.IResult");
                if (iResultTypeSymbol is null)
                {
                    return;
                }

                // Check if the return type is of Task<IResult> or Task<Result<>>. If yes, then 
                if (SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, TaskOfTSymbol))
                {
                    var subType = typedReturnType.TypeArguments[0];
                    if (subType is INamedTypeSymbol namedSubType)
                    {
                        typedReturnType = namedSubType;
                    }
                }

                if (Implements(typedReturnType, iResultTypeSymbol))
                {
                    var attributeStatusCodeToTypeMap = new Dictionary<int, ITypeSymbol>();
                    foreach (var attribute in methodSymbol.GetAttributes())
                    {
                        // check for number of arguments first --> maybe?
                        if (attribute.AttributeClass != null && attribute.AttributeClass.Equals(this.ProducesResponseSymbol, SymbolEqualityComparer.Default))
                        {
                            if (attribute.ConstructorArguments.Length == 1)
                            {
                                var statusCodeValue = (int) attribute.ConstructorArguments[0].Value;
                                attributeStatusCodeToTypeMap.Add(statusCodeValue, this._statusCodeToResultsMap[statusCodeValue]);
                            }
                            else
                            {
                                var constructorValue = attribute.ConstructorArguments[0].Value;
                                var statusCodeValue = (int) attribute.ConstructorArguments[1].Value;
                                if (constructorValue is ITypeSymbol type)
                                {
                                    // TODO Do we want a different rule for catching duplicate status codes?
                                    if (attributeStatusCodeToTypeMap.ContainsKey(statusCodeValue))
                                    {
                                        var methodDeclaration = context.Symbol.Locations;
                                        context.ReportDiagnostic(Rule, context.Symbol);
                                        return;
                                    }
                                    else
                                    {
                                        attributeStatusCodeToTypeMap.Add(statusCodeValue, type);
                                    }
                                    // TODO mark it on the attribute. Hint: attribute.ApplicationSyntaxReference.GetSyntax().GetLocation()
                                }
                            }
                        }

                        else if (attribute.AttributeClass.ConstructedFrom.Equals(this.ProducesResponseOfTSymbol, SymbolEqualityComparer.Default))
                        {
                            var statusCodeValue = (int)attribute.ConstructorArguments[0].Value;
                            var typeArgument = attribute.AttributeClass.TypeArguments[0];
                            if (typeArgument is ITypeSymbol type)
                            {
                                // TODO Do we want a different rule for catching duplicate status codes?
                                if (attributeStatusCodeToTypeMap.ContainsKey(statusCodeValue))
                                {
                                    var methodDeclaration = context.Symbol.Locations;
                                    context.ReportDiagnostic(Rule, context.Symbol);
                                    return;
                                }

                                attributeStatusCodeToTypeMap.Add(statusCodeValue, type);
                            }
                        }

                        else if (attribute.AttributeClass.ConstructedFrom.Equals(this.SwaggerResponseSymbol, SymbolEqualityComparer.Default))
                        {
                            var statusCodeValue = (int)attribute.ConstructorArguments[0].Value;
                            
                            var constructorValue = attribute.ConstructorArguments[2].Value;
                            if (constructorValue is ITypeSymbol type)
                            {
                                // TODO Do we want a different rule for catching duplicate status codes?
                                if (attributeStatusCodeToTypeMap.ContainsKey(statusCodeValue))
                                {
                                    var methodDeclaration = context.Symbol.Locations;
                                    context.ReportDiagnostic(Rule, context.Symbol);
                                    return;
                                }
                                else
                                {
                                    attributeStatusCodeToTypeMap.Add(statusCodeValue, type);
                                }
                            }
                        }
                    }

                    var methodReturnSignature = GetReturnStatusAndTypeFromMethod(typedReturnType);
                    foreach (var returnValues in methodReturnSignature)
                    {
                        if (attributeStatusCodeToTypeMap.TryGetValue(returnValues.statusCode, out var methodReturnType))
                        {
                            if (!SymbolEqualityComparer.Default.Equals(methodReturnType, returnValues.symbol))
                            {
                                // TODO improve error message
                                var methodDeclaration = context.Symbol.Locations;
                                context.ReportDiagnostic(Rule, context.Symbol);
                            }
                        }
                    }
                }
            }
        }
        
        private IEnumerable<(int statusCode, ITypeSymbol symbol)> GetReturnStatusAndTypeFromMethod(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return Enumerable.Empty<(int, ITypeSymbol)>();
            }

            // Task<>
            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, TaskOfTSymbol))
            {
                return GetReturnStatusAndTypeFromMethod(namedTypeSymbol.TypeArguments[0]);
            }

            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, this.ValueTaskOfTSymbol))
            {
                return this.GetReturnStatusAndTypeFromMethod(namedTypeSymbol.TypeArguments[0]);
            }

            // Result<OK, NotFound>
            if (this.ResultTaskOfTSymbol.Any(symbol => SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, symbol)))
            {
                return namedTypeSymbol.TypeArguments.SelectMany(this.GetReturnStatusAndTypeFromMethod);
            }

            if (this._resultsToStatusCodeMap.TryGetValue(namedTypeSymbol.ConstructedFrom, out var statusCode))
            {
                // If there is a type, then return the type, otherwise return IResult type
                return [(statusCode, namedTypeSymbol.TypeArguments.Length == 0 ? namedTypeSymbol : namedTypeSymbol.TypeArguments[0])];
            }

            return Enumerable.Empty<(int, ITypeSymbol)>();
        }

        private static bool Implements(ITypeSymbol symbol, ITypeSymbol type)
        {
            return SymbolEqualityComparer.Default.Equals(symbol, type) || symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(type, i));
        }
    }
}