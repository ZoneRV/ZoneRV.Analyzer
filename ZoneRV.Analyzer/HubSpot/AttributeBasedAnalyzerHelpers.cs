using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using INamedTypeSymbol = Microsoft.CodeAnalysis.INamedTypeSymbol;

namespace ZoneRV.Analyzer.HubSpot;

public static class AttributeBasedAnalyzerHelpers
{
    /// <summary>
    /// Resolves the actual type symbol from a type parameter name (e.g., "TEntity" -> Deal)
    /// by looking at the invocation's type arguments
    /// </summary>
    public static INamedTypeSymbol? ResolveTypeParameter(
        string typeParameterName,
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        // Find the type parameter by name in the method's type parameters
        var typeParam = methodSymbol.TypeParameters.FirstOrDefault(tp => tp.Name == typeParameterName);
        if (typeParam is null)
            return null;

        var typeParamIndex = methodSymbol.TypeParameters.IndexOf(typeParam);

        // Get the actual type argument from the invocation
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name is GenericNameSyntax genericName)
        {
            if (typeParamIndex < genericName.TypeArgumentList.Arguments.Count)
            {
                var typeArg = genericName.TypeArgumentList.Arguments[typeParamIndex];
                return semanticModel.GetSymbolInfo(typeArg).Symbol as INamedTypeSymbol;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the Properties type from an entity (first type argument of HubSpotEntityBase)
    /// </summary>
    public static INamedTypeSymbol? GetPropertiesTypeFromEntity(INamedTypeSymbol entityType)
    {
        var baseType = entityType.BaseType;
        while (baseType is not null)
        {
            if (baseType.Name == "HubSpotEntityBase" && baseType.TypeArguments.Length >= 1)
            {
                return baseType.TypeArguments[0] as INamedTypeSymbol;
            }
            baseType = baseType.BaseType;
        }
        return null;
    }

    /// <summary>
    /// Extracts all string values and their source expressions from an argument
    /// Handles literals, const references, arrays, and collection expressions
    /// </summary>
    public static IEnumerable<(string Value, ExpressionSyntax Expression, INamedTypeSymbol? SourceType)> ExtractStringValuesWithSource(ArgumentSyntax argument, SemanticModel semanticModel)
    {
        var expression = argument.Expression;

        // Handle collection initializers
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            foreach (var item in implicitArray.Initializer.Expressions)
            {
                var result = GetStringValueWithSource(item, semanticModel);
                if (result.HasValue)
                    yield return (result.Value.Value, item, result.Value.SourceType);
            }
        }
        else if (expression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer is not null)
        {
            foreach (var item in arrayCreation.Initializer.Expressions)
            {
                var result = GetStringValueWithSource(item, semanticModel);
                if (result.HasValue)
                    yield return (result.Value.Value, item, result.Value.SourceType);
            }
        }
        else if (expression is CollectionExpressionSyntax collectionExpr)
        {
            foreach (var element in collectionExpr.Elements)
            {
                if (element is ExpressionElementSyntax exprElement)
                {
                    var result = GetStringValueWithSource(exprElement.Expression, semanticModel);
                    if (result.HasValue)
                        yield return (result.Value.Value, exprElement.Expression, result.Value.SourceType);
                }
            }
        }
        else
        {
            // Single value
            var result = GetStringValueWithSource(expression, semanticModel);

            if (result.HasValue)
                yield return (result.Value.Value, expression, result.Value.SourceType);
        }
    }

    private static (string Value, INamedTypeSymbol? SourceType)? GetStringValueWithSource(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // String literal
        if (expression is LiteralExpressionSyntax literal)
        {
            var constantValue = semanticModel.GetConstantValue(literal);
            if (constantValue.HasValue && constantValue.Value is string str)
            {
                return (str, null);
            }
        }

        // Const field reference (e.g., DealProperties.AmountJson)
        var symbol = semanticModel.GetSymbolInfo(expression).Symbol;
        if (symbol is IFieldSymbol field && field.IsConst && field.HasConstantValue)
        {
            if (field.ConstantValue is string value)
            {
                return (value, field.ContainingType);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the parameter for a specific argument in an invocation
    /// </summary>
    public static IParameterSymbol? GetParameterForArgument(
        ArgumentSyntax argument,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return null;

        var argumentList = invocation.ArgumentList.Arguments;
        var argumentIndex = argumentList.IndexOf(argument);

        // Handle named arguments
        if (argument.NameColon is not null)
        {
            var paramName = argument.NameColon.Name.Identifier.Text;
            return methodSymbol.Parameters.FirstOrDefault(p => p.Name == paramName);
        }

        // Positional argument
        if (argumentIndex >= 0 && argumentIndex < methodSymbol.Parameters.Length)
        {
            return methodSymbol.Parameters[argumentIndex];
        }

        return null;
    }
}