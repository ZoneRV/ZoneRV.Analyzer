using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.DebugDisplay;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyDebugDisplayAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule1 = new DiagnosticDescriptor(
        "ZRV0002",
        Resources.ZRV0002Title,
        Resources.ZRV0002MessageFormat,
        category: "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Resources.ZRV0002DescriptionEmpty
    );
    
    private static readonly DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(
        "ZRV0002",
        Resources.ZRV0002Title,
        Resources.ZRV0002MessageFormat,
        category: "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Resources.ZRV0002DescriptionEmpty
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule1, Rule2);
    
    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        // Check if the analyzed node is an AttributeSyntax
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Get the name of the attribute
        var name = attributeSyntax.Name.ToString();

        // Look for "DebugDisplayAttribute" (full name or unqualified name)
        if (name != "DebuggerDisplay" && name != "DebuggerDisplayAttribute")
            return;

        // Retrieve the semantic model for attribute resolution
        var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
        if (attributeSymbol?.ContainingType.ToDisplayString() != "System.Diagnostics.DebuggerDisplayAttribute")
            return;

        // Find the "Value" property argument
        var argument = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault(a =>
            a.NameEquals?.Name.Identifier.ValueText == "Value" || a.NameEquals == null);

        // Check if the argument exists and if its value is empty or whitespace
        if (argument?.Expression is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            var value = literal.Token.ValueText;
            if (string.IsNullOrWhiteSpace(value))
            {
                // Report a diagnostic if the value is empty or whitespace
                var diagnostic = Diagnostic.Create(Rule1, argument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            else if (!(value.Contains("{") && value.Contains("}")))
            {
                // Report a diagnostic if the value is empty or whitespace
                var diagnostic = Diagnostic.Create(Rule2, argument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}