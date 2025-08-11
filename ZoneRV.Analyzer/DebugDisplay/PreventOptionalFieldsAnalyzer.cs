using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ZoneRV.Analyzer.DebugDisplay;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreventOptionalFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule1 = new DiagnosticDescriptor(
        "ZRV0007",
        Resources.ZRV0007Title,
        Resources.ZRV0007MessageFormat,
        category: "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Resources.ZRV0007Title
    );
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule1);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
    
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.Attribute);
    }
    
    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        var name = attributeSyntax.Name.ToString();

        if (name != "DebuggerDisplay" && name != "DebuggerDisplayAttribute")
            return;

        var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
        
        if (attributeSymbol?.ContainingType.Name != "DebuggerDisplayAttribute")
            return;

        var argument = attributeSyntax.ArgumentList?.Arguments
            .FirstOrDefault(a =>
                a.NameEquals?.Name.Identifier.ValueText == "Value" || a.NameEquals == null
                );
        
        if (argument?.Expression is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            return;
        
        var valueText = literal.Token.ValueText;

        var matches = System.Text.RegularExpressions.Regex.Matches(valueText, @"\{([^\.\{\}]+)");
        
        if (matches.Count == 0)
            return;

        var declaringSymbol = context.SemanticModel.GetDeclaredSymbol(attributeSyntax.Parent?.Parent) as INamedTypeSymbol;
        
        if (declaringSymbol == null)
            return;

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var propertyName = match.Groups[1].Value;

            var propertySymbol = declaringSymbol.GetMembers(propertyName).FirstOrDefault() as IPropertySymbol;
            
            if (propertySymbol == null)
                continue;

            var hasOptionalJsonFieldAttribute = propertySymbol
                                                    .GetAttributes()
                                                    .Any(attr => attr.AttributeClass?.Name == "OptionalPropertyAttribute");
            
            if (hasOptionalJsonFieldAttribute)
            {
                var stringLiteralSpan = literal.GetLocation().SourceSpan;
                
                var badPropertyLocation = Location.Create(
                    context.Node.SyntaxTree,
                    new TextSpan(stringLiteralSpan.Start +  match.Index + 2, match.Length - 1));
                
                var diagnostic = Diagnostic.Create(Rule1, badPropertyLocation, propertyName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}