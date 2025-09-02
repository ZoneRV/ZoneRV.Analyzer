using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace ZoneRV.Analyzer.PoorName;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class PoorNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableDictionary<string, string[]> ClassBlacklistedVariableNameRules =
        ImmutableDictionary.CreateRange(
            [
                new KeyValuePair<string, string[]>("SalesOrderRequestOptions", ["filter", "sort"]),
                new KeyValuePair<string, string[]>("CardRequestOptions", ["filter", "sort"]),
                new KeyValuePair<string, string[]>("LocationInfo", ["pos"]),
                new KeyValuePair<string, string[]>("LocationMove", ["pos"]),
                new KeyValuePair<string, string[]>("OrderedLineLocation", ["position", "pos"]),
                new KeyValuePair<string, string[]>("WorkspaceLocation", ["position", "pos"]),
                new KeyValuePair<string, string[]>("SalesOrder", ["van"]),
                new KeyValuePair<string, string[]>("ProHoStatus", ["redline"]),
                new KeyValuePair<string, string[]>("OptionalPropertyCollection", ["field"]),
                new KeyValuePair<string, string[]>("OptionalProperty", ["field"]),
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

    private static readonly DiagnosticDescriptor AsyncRule = new DiagnosticDescriptor(
        "ZRV0009",
        Resources.ZRV0009Title,
        Resources.ZRV0009Title,
        "Naming",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Resources.ZRV0009Title);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, AsyncRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(MethodDeclaration, SyntaxKind.MethodDeclaration);

        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclarator);
        
        context.RegisterSyntaxNodeAction(AnalyzeMemberDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMemberDeclaration, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMemberDeclaration, SyntaxKind.Parameter);
        
        context.RegisterSyntaxNodeAction(AnalyzeSimpleLambdaDeclaration, SyntaxKind.SimpleLambdaExpression);
        context.RegisterSyntaxNodeAction(AnalyzeParenthesizedLambdaDeclaration, SyntaxKind.ParenthesizedLambdaExpression);
        
        context.RegisterSyntaxNodeAction(AnalyzeForeachVariableDeclaration, SyntaxKind.ForEachStatement);
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