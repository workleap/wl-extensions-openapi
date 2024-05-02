using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Workleap.Extensions.OpenAPI.Analyzers.Internals;

namespace Workleap.Extensions.OpenAPI.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CompareTypedResultWithAnnotationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor AnnotationMustMatchTypedResult = new(
        id: RuleIdentifiers.MismatchResponseTypeWithAnnotation,
        title: "Mismatch between annotation return type and endpoint return type",
        messageFormat: "Mismatch between annotation return type and endpoint return type",
        category: RuleCategories.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: RuleIdentifiers.HelpUri);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AnnotationMustMatchTypedResult);

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
        private INamedTypeSymbol? ProducesResponseSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute");
        private INamedTypeSymbol? ProducesResponseOfTSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute`1");
        private INamedTypeSymbol? SwaggerResponseSymbol { get; } = compilation.GetTypeByMetadataName("Swashbuckle.AspNetCore.Annotations.SwaggerResponseAttribute");

        private INamedTypeSymbol? ResultSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.IResult");

        public bool IsValid => this.ResultSymbol is not null && this.ProducesResponseSymbol is not null;

        private INamedTypeSymbol?[] ResultTaskOfTSymbol { get; } =
        [
            compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`2"),
            compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`3"),
            compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`4"),
            compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`5"),
            compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.HttpResults.Results`6"),
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

        public void ValidateEndpointResponseType(SymbolAnalysisContext context)
        {
            if (context.Symbol.GetAttributes().Length == 0)
            {
                return;
            }

            var methodSymbol = (IMethodSymbol)context.Symbol;
            var typedReturnType = this.GetReturnTypeSymbol(methodSymbol);
            if (typedReturnType != null && this.IsImplementingIResult(typedReturnType))
            {
                var methodSignatureStatusCodeToType = this.GetMethodReturnStatusCodeToType(typedReturnType);

                foreach (var attribute in methodSymbol.GetAttributes())
                {
                    this.ValidateAnnotationWithTypedResult(context, attribute, methodSignatureStatusCodeToType);
                }
            }
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
            if (SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, this.TaskOfTSymbol) ||
                SymbolEqualityComparer.Default.Equals(typedReturnType.ConstructedFrom, this.ValueTaskOfTSymbol))
            {
                var subType = typedReturnType.TypeArguments[0];
                if (subType is INamedTypeSymbol namedSubType)
                {
                    typedReturnType = namedSubType;
                }
            }

            return typedReturnType;
        }

        private void ValidateAnnotationWithTypedResult(SymbolAnalysisContext context, AttributeData attribute,
            Dictionary<int, List<ITypeSymbol>> methodSignatureStatusCodeToTypeMap)
        {
            if (attribute.AttributeClass == null || attribute.ConstructorArguments.Length == 0)
            {
                return;
            }

            // For the annotations [ProducesResponseType(<StatusCode>)] and [ProducesResponseType(<typeof()>, <StatusCode>)] 
            if (attribute.AttributeClass.Equals(this.ProducesResponseSymbol, SymbolEqualityComparer.Default))
            {
                if (attribute.ConstructorArguments.Length == 1)
                {
                    if (attribute.ConstructorArguments[0].Value is int statusCodeValue)
                    {
                        if (this._statusCodeToResultsMap.TryGetValue(statusCodeValue, out var type))
                        {
                            ValidateAnnotationForTypeMismatch(attribute, statusCodeValue, type, methodSignatureStatusCodeToTypeMap, context);
                        }
                    }
                }
                else if (attribute.ConstructorArguments[1].Value is int statusCodeValue && attribute.ConstructorArguments[0].Value is ITypeSymbol type)
                {
                    ValidateAnnotationForTypeMismatch(attribute, statusCodeValue, type, methodSignatureStatusCodeToTypeMap, context);
                }
            }
            // For the annotations [ProducesResponseType<T>(<StatusCode>)]
            else if (attribute.AttributeClass.ConstructedFrom.Equals(this.ProducesResponseOfTSymbol, SymbolEqualityComparer.Default))
            {
                if (attribute.ConstructorArguments[0].Value is int statusCodeValue)
                {
                    ValidateAnnotationForTypeMismatch(attribute, statusCodeValue, attribute.AttributeClass.TypeArguments[0], methodSignatureStatusCodeToTypeMap, context);
                }
            }

            // For the annotations [SwaggerResponse(<StatusCode>, "description", <typeof()>]
            else if (attribute.AttributeClass.ConstructedFrom.Equals(this.SwaggerResponseSymbol, SymbolEqualityComparer.Default))
            {
                if (attribute.ConstructorArguments.Length > 2 && attribute.ConstructorArguments[0].Value is int statusCodeValue && attribute.ConstructorArguments[2].Value is ITypeSymbol type)
                {
                    ValidateAnnotationForTypeMismatch(attribute, statusCodeValue, type, methodSignatureStatusCodeToTypeMap, context);
                }
            }
        }

        // Result<Ok<type>, Notfound>
        private Dictionary<int, List<ITypeSymbol>> GetMethodReturnStatusCodeToType(ITypeSymbol methodSymbol)
        {
            var methodReturnSignatures = this.ExtractStatusCodeAndResultFromMethodReturn(methodSymbol);
            var methodSignatureStatusCodeToTypeMap = new Dictionary<int, List<ITypeSymbol>>();
            foreach (var returnValues in methodReturnSignatures)
            {
                if (methodSignatureStatusCodeToTypeMap.TryGetValue(returnValues.statusCode, out var returnTypeSymbols))
                {
                    returnTypeSymbols.Add(returnValues.symbol);
                }
                else
                {
                    methodSignatureStatusCodeToTypeMap.Add(returnValues.statusCode, [returnValues.symbol]);
                }
            }

            return methodSignatureStatusCodeToTypeMap;
        }

        private IEnumerable<(int statusCode, ITypeSymbol symbol)> ExtractStatusCodeAndResultFromMethodReturn(ITypeSymbol methodSymbol)
        {
            if (methodSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return Enumerable.Empty<(int, ITypeSymbol)>();
            }

            // Result<OK, NotFound>
            if (this.ResultTaskOfTSymbol.Any(symbol => SymbolEqualityComparer.Default.Equals(namedTypeSymbol.ConstructedFrom, symbol)))
            {
                return namedTypeSymbol.TypeArguments.SelectMany(this.ExtractStatusCodeAndResultFromMethodReturn);
            }

            if (this._resultsToStatusCodeMap.TryGetValue(namedTypeSymbol.ConstructedFrom, out var statusCode))
            {
                // If there is a type, then return the type, otherwise return IResult type
                return [(statusCode, namedTypeSymbol.TypeArguments.Length == 0 ? namedTypeSymbol : namedTypeSymbol.TypeArguments[0])];
            }

            return Enumerable.Empty<(int, ITypeSymbol)>();
        }

        private static void ValidateAnnotationForTypeMismatch(AttributeData attribute, int statusCodeFromAnnotation, ITypeSymbol typeFromAnnotation,
            Dictionary<int, List<ITypeSymbol>> methodReturnStatusCodeTypes, SymbolAnalysisContext context)
        {
            if (attribute.ApplicationSyntaxReference is null)
            {
                return;
            }

            var attributeLocation = attribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken).GetLocation();
            if (!methodReturnStatusCodeTypes.TryGetValue(statusCodeFromAnnotation, out var mappedType))
            {
                return;
            }

            if (!mappedType.Any(type => SymbolEqualityComparer.Default.Equals(type, typeFromAnnotation)))
            {
                context.ReportDiagnostic(AnnotationMustMatchTypedResult, attributeLocation);
            }
        }

        private bool IsImplementingIResult(ITypeSymbol currentClassSymbol)
        {
            var resultSymbol = this.ResultSymbol;
            return SymbolEqualityComparer.Default.Equals(currentClassSymbol, resultSymbol) || currentClassSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(resultSymbol, i));
        }
    }
}