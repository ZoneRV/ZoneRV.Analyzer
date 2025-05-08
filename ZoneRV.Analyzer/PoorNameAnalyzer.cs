using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace ZoneRV.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PoorNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableDictionary<string, string[]> ClassBlacklistedVariableNameRules =
        ImmutableDictionary.CreateRange(
            [
                new KeyValuePair<string, string[]>("SalesOrderRequestOptions", ["filter", "sort"]),
                new KeyValuePair<string, string[]>("CardRequestOptions", ["filter", "sort"]),
                new KeyValuePair<string, string[]>("LocationInfo", ["pos"]),
                new KeyValuePair<string, string[]>("LocationMove", ["pos"]),
                new KeyValuePair<string, string[]>("OrderedLineLocation", ["pos"]),
                new KeyValuePair<string, string[]>("WorkspaceLocation", ["pos"]),
                new KeyValuePair<string, string[]>("SalesOrder", ["van"]),
                new KeyValuePair<string, string[]>("ProHoStatus", ["redline"]),
            ]
        );

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "ZRV0006",
        Resources.ZRV0006Title,
        Resources.ZRV0006MessageFormat,
        "Naming",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Resources.ZRV0006Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclarator);
        
        context.RegisterSyntaxNodeAction(AnalyzeMemberDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMemberDeclaration, SyntaxKind.FieldDeclaration);

    }

    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variableDeclarator = (VariableDeclaratorSyntax)context.Node;

        string variableName = variableDeclarator.Identifier.Text;

        var variableDeclaration = variableDeclarator.Parent as VariableDeclarationSyntax;
        if (variableDeclaration == null) return;

        var typeInfo = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;
        if (typeInfo == null) return;

        string className = typeInfo.Name;

        if (ClassBlacklistedVariableNameRules.TryGetValue(className, out var blacklistedNames))
        {
            var badName = GetBlacklistedName(variableName, blacklistedNames);
            
            if (badName is not null)
            {
                var diagnostic = Diagnostic.Create(Rule, variableDeclarator.Identifier.GetLocation(), className, variableName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
    
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

            default:
                return;
        }

        if (memberName is null || className is null) return;

        if (ClassBlacklistedVariableNameRules.TryGetValue(className, out var blacklistedNames))
        {
            var badName = GetBlacklistedName(memberName, blacklistedNames);
            
            if (badName is not null)
            {
                var diagnostic = Diagnostic.Create(Rule, location, className, memberName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }


    private string? GetBlacklistedName(string variableName, string[] blacklistedNames)
    {
        foreach (var name in blacklistedNames)
        {
            if (variableName.ToLower().Contains(name.ToLower()))
            {
                return name;
            }
        }
        return null;
    }
}