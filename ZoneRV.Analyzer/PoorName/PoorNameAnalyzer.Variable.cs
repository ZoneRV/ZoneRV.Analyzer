using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.PoorName;

public partial class PoorNameAnalyzer
{
    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (VariableDeclaratorSyntax)context.Node;

        string variableName = variableDeclarator.Identifier.Text;

        var variableDeclaration = variableDeclarator.Parent as VariableDeclarationSyntax;
        if (variableDeclaration == null)
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;
        if (typeInfo == null)
            return;

        string className = typeInfo.Name;

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