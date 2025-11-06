using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace ZoneRV.Analyzer.HubSpot;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExpectConstInArgumentsAnalyzer : DiagnosticAnalyzer
{
    private const string ExpectConstPropertiesFromAttribute = "ExpectConstPropertiesFromAttribute";
    private const string ExpectConstAssociationsFromAttribute = "ExpectConstAssociationsFromAttribute";

    private static readonly DiagnosticDescriptor Rule1 = new (
        "ZRVHS01",
        Resources.ZRVHS01Title,
        Resources.ZRVHS01MessageFormat,
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Resources.ZRVHS01Description);

    private static readonly DiagnosticDescriptor Rule2 = new (
        "ZRVHS02",
        Resources.ZRVHS02Title,
        Resources.ZRVHS02MessageFormat,
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Resources.ZRVHS02Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule1, Rule2];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            var parameter = AttributeBasedAnalyzerHelpers.GetParameterForArgument(
                argument, invocation, semanticModel);

            if (parameter is null)
                continue;

            var attribute = parameter.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.Name is ExpectConstPropertiesFromAttribute ||
                a.AttributeClass?.Name is ExpectConstAssociationsFromAttribute);
            if (attribute is null)
                continue;


            var typeParamName = attribute.ConstructorArguments.FirstOrDefault().Value as string;

            if (typeParamName is null)
                continue;

            // Resolve the actual type from the generic argument
            var entityType = AttributeBasedAnalyzerHelpers.ResolveTypeParameter(
                typeParamName, methodSymbol, invocation, semanticModel);

            // If resolution failed, try resolving it as a direct type name (for non-generic methods)
            if (entityType is null)
            {
                var typeInfo = semanticModel.GetTypeInfo(invocation.Expression);
                // Try to find the type in the current context
                entityType = semanticModel.LookupSymbols(invocation.SpanStart, name: typeParamName)
                    .OfType<INamedTypeSymbol>()
                    .FirstOrDefault();
            }

            if (entityType is null)
                continue;

            // Get the expected Properties or Associations type
            var expectedType = attribute.AttributeClass?.Name is ExpectConstPropertiesFromAttribute
                ? AttributeBasedAnalyzerHelpers.GetPropertiesTypeFromEntity(entityType)
                : AttributeBasedAnalyzerHelpers.GetAssociationsTypeFromEntity(entityType);

            if (expectedType is null)
                continue;

            // Validate each string value in the argument
            foreach (var (value, expr, sourceType) in
                AttributeBasedAnalyzerHelpers.ExtractStringValuesWithSource(argument, semanticModel))
            {
                if (sourceType is null) // Is string
                {
                    var validConstant = expectedType.GetMembers()
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault(f
                            =>
                             f.IsConst &&
                             f.DeclaredAccessibility == Accessibility.Public &&
                             f.Type.SpecialType == SpecialType.System_String &&
                             f.HasConstantValue &&
                             f.ConstantValue is string cString &&
                             cString == value
                            );

                    if (validConstant is not null)
                    {
                        Dictionary<string, string?> properties = [];

                        properties.Add("constant", $"{expectedType.Name}.{validConstant.Name}");

                        var diagnostic = Diagnostic.Create(
                            Rule2,
                            expr.GetLocation(),
                            properties: properties.ToImmutableDictionary() ,
                            expectedType.Name,
                            validConstant.Name,
                            value);

                        context.ReportDiagnostic(diagnostic);
                    }
                }

                // Check if the source type matches the expected type
                else if (!SymbolEqualityComparer.Default.Equals(sourceType, expectedType))
                {
                    // Only report diagnostic if the source type implements IProperties or IAssociations
                    if (AttributeBasedAnalyzerHelpers.InheritsFromIPropertiesOrIAssociations(sourceType))
                    {
                        var diagnostic = Diagnostic.Create(
                            Rule1,
                            expr.GetLocation(),
                            sourceType.Name,
                            expectedType.Name);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}