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

    // maybe a liste 
    // registe

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

        // if we do two compilations --> this wouldn't work.
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var analyzerContext = new AnalyzerContext(compilationContext.Compilation);

            compilationContext.RegisterSymbolAction(analyzerContext.AnalyzeClassDeclaration, SymbolKind.Method);
        });
    }


    // private static Type GetTypeFromSymbol(ITypeSymbol symbol)
    // {
    //     
    //     return Type.GetType(symbol.Name)
    // }
    private sealed class AnalyzerContext(Compilation compilation)
    {
        public INamedTypeSymbol? TaskSymbol { get; } = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.Task");
        public INamedTypeSymbol? TaskOfTSymbol { get; } = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.Task`1");
        public INamedTypeSymbol? ValueTaskSymbol { get; } = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.ValueTask");
        public INamedTypeSymbol? ValueTaskOfTSymbol { get; } = compilation.GetBestTypeByMetadataName("System.Threading.Tasks.ValueTask`1");


        public INamedTypeSymbol?[] ResultTaskOfTSymbol { get; } =
        [
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`2"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`3"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`4"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`5"),
            compilation.GetBestTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`6"),
        ];


        private readonly Dictionary<ITypeSymbol, int> _statusIResultsMap = Initialize(compilation);

        private static Dictionary<ITypeSymbol, int> Initialize(Compilation compilation)
        {
            // TODO initialize with other response types
            var dictionary = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
            Add("Microsoft.AspNetCore.Http.HttpResults.Ok", 200);
            Add("Microsoft.AspNetCore.Http.HttpResults.Ok`1", 200);
            Add("Microsoft.AspNetCore.Http.HttpResults.NotFound", 404);
            Add("Microsoft.AspNetCore.Http.HttpResults.NotFound`1", 404);

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

        public void AnalyzeClassDeclaration(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;
            var returnType = methodSymbol.ReturnType;

            if (returnType is INamedTypeSymbol typedReturnType)
            {
                // typedReturnType.Const
                // 1. Detect if we have IResult response
                // 2. Then check if we have annotations of type ProducesResponseType or SwaggerResponse type
                // 3. If yes, then extract generic response type from both annotations and method return type signature
                // figure out how to get response code, how to get type of response type? --> Swagger would be different.
                // three use cases, Produces/Produces<T>/Swagger
                // 4. Match case by case if it is equal.
                // var iTypeSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.IResult")!;
                // --> may need to validate duplicate definitions
                var iResultTypeSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.IResult");
                if (iResultTypeSymbol is null)
                {
                    return;
                }

                if (Implements(typedReturnType, iResultTypeSymbol))
                {
                    // For each of get 
                    var producesResponseTypeSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute");
                    // we put the number of generic arguments after the backtick
                    var producesResponseTypeOfTSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute`1");
                    var swaggerResponseTypeSymbol = context.Compilation.GetTypeByMetadataName("Swashbuckle.AspNetCore.Annotations.SwaggerResponseAttribute");

                    var dictionaryCodeToType = new Dictionary<int, ITypeSymbol>();
                    foreach (var attribute in methodSymbol.GetAttributes())
                    {
                        // check for number of arguments first --> maybe?
                        if (attribute.AttributeClass.Equals(producesResponseTypeSymbol, SymbolEqualityComparer.Default))
                        {
                            var constructorValue = attribute.ConstructorArguments[0].Value;
                            var statusCodeValue = (int)attribute.ConstructorArguments[1].Value;
                            if (constructorValue is ITypeSymbol type)
                            {
                                // --> handle duplicate cases. --> should add a diagnostic
                                dictionaryCodeToType.Add(statusCodeValue, type);
                            }
                        }

                        // List<T> --> attributeclass is a list of string. What we can do: take the attribute class --> ConstructedFrom, take the generic version of the Type
                        // what is the type? --> We want to take the string.
                        else if (attribute.AttributeClass.ConstructedFrom.Equals(producesResponseTypeOfTSymbol, SymbolEqualityComparer.Default))
                        {
                            // type arguments are declaration List<T> --> parameter is T, but argument is string
                            // we get the type correctly
                            var typeArgument = attribute.AttributeClass.TypeArguments[0];
                        }

                        else if (attribute.AttributeClass.ConstructedFrom.Equals(swaggerResponseTypeSymbol, SymbolEqualityComparer.Default))
                        {
                        }
                        // dig in arguments --> Task/ValueTask --> TypeArguments --> Results of something.
                        // --> may need to create a dictionary of Ok, badrequest, notfound --> switch case. --> this is the simples solution
                        // 

                        // typeof --> DLL will not exist --> this will not work.
                        // Value is actually the type needed here. --> could also be a generic type.
                    }
                    
                    var methodReturnSignature = GetReturnStatusAndTypeFromMethod(typedReturnType);
                    foreach (var returnValues in methodReturnSignature)
                    {
                        if (dictionaryCodeToType.TryGetValue(returnValues.statusCode, out var methodReturnType))
                        {
                            if (!SymbolEqualityComparer.Default.Equals(methodReturnType, returnValues.symbol))
                            {
                                // TODO improve error message --> 
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
            
            if (this._statusIResultsMap.TryGetValue(namedTypeSymbol.ConstructedFrom, out var statusCode))
            {
                return [(statusCode, namedTypeSymbol.TypeArguments.Length == 0 ? null : namedTypeSymbol.TypeArguments[0])];
            }

            return Enumerable.Empty<(int, ITypeSymbol)>();
        }

        private static bool Implements(ITypeSymbol symbol, ITypeSymbol type)
        {
            return SymbolEqualityComparer.Default.Equals(symbol, type) || symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(type, i));
        }
    }
}