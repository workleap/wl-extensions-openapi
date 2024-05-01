using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Workleap.Extensions.OpenAPI.Analyzers.Internals;

namespace Workleap.Extensions.OpenAPI.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CompareTypedResultWithAnnotationAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "WLOAS001";

    internal static readonly DiagnosticDescriptor AnnotationMustMatchTypedResult = new(
        id: DiagnosticId,
        title: "Mismatch between annotation return type and endpoint return type",
        messageFormat: "Mismatch between annotation return type and endpoint return type",
        category: "OpenAPI",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AnnotationMustMatchTypedResult);

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
            var dictionary = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
            Add("Microsoft.AspNetCore.Http.HttpResults.Ok", 200);
            Add("Microsoft.AspNetCore.Http.HttpResults.Ok`1", 200);
            Add("Microsoft.AspNetCore.Http.HttpResults.Created", 201);
            Add("Microsoft.AspNetCore.Http.HttpResults.Created`1", 201);
            Add("Microsoft.AspNetCore.Http.HttpResults.CreatedAtRoute", 201);
            Add("Microsoft.AspNetCore.Http.HttpResults.CreatedAtRoute`1", 201);
            Add("Microsoft.AspNetCore.Http.HttpResults.Accepted", 202);
            Add("Microsoft.AspNetCore.Http.HttpResults.Accepted`1", 202);
            Add("Microsoft.AspNetCore.Http.HttpResults.AcceptedAtRoute", 202);
            Add("Microsoft.AspNetCore.Http.HttpResults.AcceptedAtRoute`1", 202);
            Add("Microsoft.AspNetCore.Http.HttpResults.NoContent", 204);
            Add("Microsoft.AspNetCore.Http.HttpResults.BadRequest", 400);
            Add("Microsoft.AspNetCore.Http.HttpResults.BadRequest`1", 400);
            Add("Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult", 401);
            Add("Microsoft.AspNetCore.Http.HttpResults.NotFound", 404);
            Add("Microsoft.AspNetCore.Http.HttpResults.NotFound`1", 404);
            Add("Microsoft.AspNetCore.Http.HttpResults.Conflict", 409);
            Add("Microsoft.AspNetCore.Http.HttpResults.Conflict`1", 409);
            Add("Microsoft.AspNetCore.Http.HttpResults.UnprocessableEntity", 422);
            Add("Microsoft.AspNetCore.Http.HttpResults.UnprocessableEntity`T", 422);
            // Will be Supported in .NET 9
            Add("Microsoft.AspNetCore.Http.HttpResults.InternalServerError", 500);
            Add("Microsoft.AspNetCore.Http.HttpResults.InternalServerError`1", 500);
            // Workleap's definition of the InternalServerError type result for other .NET versions
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
            var dictionary = new Dictionary<int, ITypeSymbol>();
            Add(200, "Microsoft.AspNetCore.Http.HttpResults.Ok");
            Add(201, "Microsoft.AspNetCore.Http.HttpResults.Created");
            Add(202, "Microsoft.AspNetCore.Http.HttpResults.Accepted");
            Add(204, "Microsoft.AspNetCore.Http.HttpResults.NoContent");
            Add(400, "Microsoft.AspNetCore.Http.HttpResults.BadRequest");
            Add(401, "Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult");
            Add(404, "Microsoft.AspNetCore.Http.HttpResults.NotFound");
            Add(409, "Microsoft.AspNetCore.Http.HttpResults.Conflict");
            Add(422, "Microsoft.AspNetCore.Http.HttpResults.UnprocessableEntity");
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

                // Check if the return type is of Task<IResult> or Task<Result<>>. If yes, then keep the inner type.
                if (SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, this.TaskOfTSymbol))
                {
                    var subType = typedReturnType.TypeArguments[0];
                    if (subType is INamedTypeSymbol namedSubType)
                    {
                        typedReturnType = namedSubType;
                    }
                }

                if (Implements(typedReturnType, iResultTypeSymbol))
                {
                    var methodSignatureStatusCodeToTypeMap = this.GetMethodReturnStatusCodeToTypeMap(typedReturnType);

                    foreach (var attribute in methodSymbol.GetAttributes())
                    {
                        this.ValidateAnnotationWithTypedResult(context, attribute, methodSignatureStatusCodeToTypeMap);
                    }
                }
            }
        }

        private void ValidateAnnotationWithTypedResult(SymbolAnalysisContext context, AttributeData attribute,
            Dictionary<int, ITypeSymbol> methodSignatureStatusCodeToTypeMap)
        {
            if (attribute.AttributeClass != null)
            {
                if (attribute.AttributeClass.Equals(this.ProducesResponseSymbol, SymbolEqualityComparer.Default))
                {
                    if (attribute.ConstructorArguments.Length == 1)
                    {
                        if (attribute.ConstructorArguments[0].Value is int statusCodeValue)
                        {
                            var type = this._statusCodeToResultsMap[statusCodeValue];
                            ValidateAnnotationForTypeMismatch(context, methodSignatureStatusCodeToTypeMap, statusCodeValue, type, attribute);
                        }
                    }
                    else
                    {
                        if (attribute.ConstructorArguments[1].Value is int statusCodeValue && attribute.ConstructorArguments[0].Value is ITypeSymbol type)
                        {
                            ValidateAnnotationForTypeMismatch(context, methodSignatureStatusCodeToTypeMap, statusCodeValue, type, attribute);
                        }
                    }
                }

                else if (attribute.AttributeClass.ConstructedFrom.Equals(this.ProducesResponseOfTSymbol, SymbolEqualityComparer.Default))
                {
                    if (attribute.ConstructorArguments[0].Value is int statusCodeValue)
                    {
                        ValidateAnnotationForTypeMismatch(context, methodSignatureStatusCodeToTypeMap, statusCodeValue, attribute.AttributeClass.TypeArguments[0], attribute);
                    }
                }

                else if (attribute.AttributeClass.ConstructedFrom.Equals(this.SwaggerResponseSymbol, SymbolEqualityComparer.Default))
                {
                    if (attribute.ConstructorArguments[0].Value is int statusCodeValue && attribute.ConstructorArguments[2].Value is ITypeSymbol type)
                    {
                        ValidateAnnotationForTypeMismatch(context, methodSignatureStatusCodeToTypeMap, statusCodeValue, type, attribute);
                    }
                }
            }
        }

        private Dictionary<int, ITypeSymbol> GetMethodReturnStatusCodeToTypeMap(ITypeSymbol methodReturnSignature)
        {
            var methodReturnSignatures = this.GetReturnStatusAndTypeFromMethodReturn(methodReturnSignature);
            var methodSignatureStatusCodeToTypeMap = new Dictionary<int, ITypeSymbol>();
            foreach (var returnValues in methodReturnSignatures)
            {
                if (!methodSignatureStatusCodeToTypeMap.ContainsKey(returnValues.statusCode))
                {
                    methodSignatureStatusCodeToTypeMap.Add(returnValues.statusCode, returnValues.symbol);
                }
            }

            return methodSignatureStatusCodeToTypeMap;
        }

        private IEnumerable<(int statusCode, ITypeSymbol symbol)> GetReturnStatusAndTypeFromMethodReturn(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return Enumerable.Empty<(int, ITypeSymbol)>();
            }

            // Task<>
            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, this.TaskOfTSymbol))
            {
                return this.GetReturnStatusAndTypeFromMethodReturn(namedTypeSymbol.TypeArguments[0]);
            }

            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, this.ValueTaskOfTSymbol))
            {
                return this.GetReturnStatusAndTypeFromMethodReturn(namedTypeSymbol.TypeArguments[0]);
            }

            // Result<OK, NotFound>
            if (this.ResultTaskOfTSymbol.Any(symbol => SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, symbol)))
            {
                return namedTypeSymbol.TypeArguments.SelectMany(this.GetReturnStatusAndTypeFromMethodReturn);
            }

            if (this._resultsToStatusCodeMap.TryGetValue(namedTypeSymbol.ConstructedFrom, out var statusCode))
            {
                // If there is a type, then return the type, otherwise return IResult type
                return [(statusCode, namedTypeSymbol.TypeArguments.Length == 0 ? namedTypeSymbol : namedTypeSymbol.TypeArguments[0])];
            }

            return Enumerable.Empty<(int, ITypeSymbol)>();
        }

        private static void ValidateAnnotationForTypeMismatch(SymbolAnalysisContext context,
            Dictionary<int, ITypeSymbol> methodSignatureStatusCodeToTypeMap, int statusCodeValue, ITypeSymbol type, AttributeData attribute)
        {
            if (attribute.ApplicationSyntaxReference is null)
            {
                return;
            }

            var attributeLocation = attribute.ApplicationSyntaxReference.GetSyntax().GetLocation();
            if (methodSignatureStatusCodeToTypeMap.TryGetValue(statusCodeValue, out var mappedType))
            {
                if (!SymbolEqualityComparer.Default.Equals(mappedType, type))
                {
                    context.ReportDiagnostic(AnnotationMustMatchTypedResult, attributeLocation);
                }
            }
            else
            {
                context.ReportDiagnostic(AnnotationMustMatchTypedResult, attributeLocation);
            }
        }

        private static bool Implements(ITypeSymbol symbol, ITypeSymbol type)
        {
            return SymbolEqualityComparer.Default.Equals(symbol, type) || symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(type, i));
        }
    }
}