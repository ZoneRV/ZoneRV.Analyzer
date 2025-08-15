using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.PoorName;

public partial class PoorNameAnalyzer
{
    private void AnalyzeSimpleLambdaDeclaration(SyntaxNodeAnalysisContext context)
    {
        var lambdaDeclarator = (SimpleLambdaExpressionSyntax)context.Node;

        var lambdaParameter = lambdaDeclarator.Parameter;
        var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(lambdaParameter);

        var diagnostic = CreateParameterDiagnostic(parameterSymbol, lambdaParameter);
        
        if(diagnostic is not null)
            context.ReportDiagnostic(diagnostic);
    }
    
    private void AnalyzeParenthesizedLambdaDeclaration(SyntaxNodeAnalysisContext context)
    {
        var lambdaDeclarator = (ParenthesizedLambdaExpressionSyntax)context.Node;

        foreach (var parameter in lambdaDeclarator.ParameterList.Parameters)
        {
            var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameter);

            var diagnostic = CreateParameterDiagnostic(parameterSymbol, parameter);
        
            if(diagnostic is not null)
                context.ReportDiagnostic(diagnostic);
        }
    }

    private Diagnostic? CreateParameterDiagnostic(ISymbol? symbol, ParameterSyntax syntax)
    {
        var    typeInfo   = (symbol as IParameterSymbol)?.Type;
        string lambdaName = syntax.Identifier.Text;
        
        if (typeInfo == null)
            return null;

        string className = typeInfo.Name;

        if (!ClassBlacklistedVariableNameRules.TryGetValue(className, out var blacklistedNames))
            return null;
        
        var badName = GetBlacklistedName(lambdaName, blacklistedNames);

        if (badName is null)
            return null;
            
        return Diagnostic.Create(Rule, syntax.Identifier.GetLocation(), className, badName);
    }
}