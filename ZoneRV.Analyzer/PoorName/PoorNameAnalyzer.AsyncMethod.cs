using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.PoorName;

public partial class PoorNameAnalyzer
{
    private void MethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclarator = (MethodDeclarationSyntax)context.Node;

        // Syntactic check: looks for the 'async' modifier directly
        var isAsyncBySyntax = methodDeclarator.Modifiers.Any(SyntaxKind.AsyncKeyword);

        // Semantic check: asks the symbol if it's async (requires SemanticModel)
        var symbol          = context.SemanticModel.GetDeclaredSymbol(methodDeclarator);
        var isAsyncBySymbol = symbol?.IsAsync == true;

        var isAsync = isAsyncBySyntax || isAsyncBySymbol;

        if (!methodDeclarator.Identifier.ValueText.EndsWith("async", StringComparison.OrdinalIgnoreCase) && isAsync)
        {
            var diagnostic = Diagnostic.Create(AsyncRule, methodDeclarator.Identifier.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }
}