using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.PoorName;

public partial class PoorNameAnalyzer
{
    private void AnalyzeMemberDeclaration(SyntaxNodeAnalysisContext context)
    {
        string? memberName = null;
        string? className  = null;

        Location location;

        switch (context.Node)
        {
            case PropertyDeclarationSyntax propertyDeclaration:
                memberName = propertyDeclaration.Identifier.Text;
                location   = propertyDeclaration.Identifier.GetLocation();
                className  = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type).Type?.Name;
                break;

            case ParameterSyntax parameterSyntax:
                memberName = parameterSyntax.Identifier.Text;
                location   = parameterSyntax.Identifier.GetLocation();

                if (parameterSyntax.Type is null)
                    return;

                className = context.SemanticModel.GetTypeInfo(parameterSyntax.Type).Type?.Name;
                break;

            default:
                return;
        }

        if (memberName is null || className is null)
            return;

        if (ClassBlacklistedVariableNameRules.TryGetValue(className, out var blacklistedNames))
        {
            var badName = GetBlacklistedName(memberName, blacklistedNames);

            if (badName is not null)
            {
                var diagnostic = Diagnostic.Create(Rule, location, className, badName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}