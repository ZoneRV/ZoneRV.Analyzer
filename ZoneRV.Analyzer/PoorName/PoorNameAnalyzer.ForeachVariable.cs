using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.PoorName;

public partial class PoorNameAnalyzer
{
    private void AnalyzeForeachVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (ForEachStatementSyntax)context.Node;

        string variableName = variableDeclarator.Identifier.Text;
        
        var symbolInfo = context.SemanticModel.GetDeclaredSymbol(variableDeclarator) as ILocalSymbol;

        string className = symbolInfo!.Type.Name;

        if (ClassBlacklistedVariableNameRules.TryGetValue(className, out var blacklistedNames))
        {
            var badName = GetBlacklistedName(variableName, blacklistedNames);

            if (badName is not null)
            {
                var diagnostic = Diagnostic.Create(Rule, variableDeclarator.Identifier.GetLocation(), className, badName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}