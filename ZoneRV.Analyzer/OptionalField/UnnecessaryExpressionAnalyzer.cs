﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.OptionalField;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnnecessaryExpressionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor RuleFull = new DiagnosticDescriptor(
        "ZRV0004",
        Resources.ZRV0004,
        Resources.ZRV0004,
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary]
    );
    
    private static readonly DiagnosticDescriptor RuleTrailing = new DiagnosticDescriptor(
        "ZRV0005",
        Resources.ZRV0005,
        Resources.ZRV0005,
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary]
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [RuleFull, RuleTrailing];
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == "WithPropertiesFromType")
        {
            foreach (var expression in invocation.ArgumentList.Arguments.Select(x => x.Expression as LambdaExpressionSyntax))
            {
                ValidateLambdaExpression(expression, context);
            }
        }
    }
    
    private static void ValidateLambdaExpression(LambdaExpressionSyntax lambdaExpression, SyntaxNodeAnalysisContext context)
    {
        if (lambdaExpression.Body is ExpressionSyntax body)
        {
            List<IdentifierNameSyntax> decendants = body.DescendantNodes().OfType<IdentifierNameSyntax>().Skip(1).ToList();
            
            List<IdentifierNameSyntax> nodes = [];
            
            var lastNode = decendants.Last();

            var lastSymbol = context.SemanticModel.GetSymbolInfo(lastNode, context.CancellationToken).Symbol;
            
            if(lastSymbol is IPropertySymbol lastPropertySymbol && Utils.HasAttribute(lastPropertySymbol, "OptionalJsonFieldAttribute"))
                return;
            
            nodes.Add(lastNode);

            foreach (var node in decendants.Take(decendants.Count - 1).Reverse())
            {
                var symbol = context.SemanticModel.GetSymbolInfo(node, context.CancellationToken).Symbol;
                
                if (symbol is not IPropertySymbol propertySymbol || !Utils.HasAttribute(propertySymbol, "OptionalJsonFieldAttribute"))
                {
                    nodes.Add(node);
                }
                else
                    break;
            }

            Location location;
            bool     isFullExpression = false;
            
            if(nodes.Count == decendants.Count)
            {
                isFullExpression = true;
                location         = lambdaExpression.GetLocation();
            }
            
            else
            {
                var parent = nodes.Last().Parent as MemberAccessExpressionSyntax;
                
                var start = nodes.Last().SpanStart;
                var end = lastNode.Span.End;

                if (parent != null)
                {
                    // Include the dot in the span
                    start = parent.OperatorToken.Span.Start;
                }
                
                location = Location.Create(
                    lastNode.SyntaxTree,
                    Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(start, end)
                );
            }

            if (isFullExpression)
                context.ReportDiagnostic(Diagnostic.Create(RuleFull, location));
            
            else
                context.ReportDiagnostic(Diagnostic.Create(RuleTrailing, location));
        }
    }
}