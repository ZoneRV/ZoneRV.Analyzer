using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.NullEquality;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullCheckAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UseIsPatternForNullCheck = new DiagnosticDescriptor(
        "ZRV0008",
        Resources.ZRV0008Title,
        Resources.ZRV0008MessageFormat,
        "Style",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using 'is' patterns for null checks is more explicit and consistent with modern C# style.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(UseIsPatternForNullCheck);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;
        
        // Check if one side is null literal
        var leftIsNull  = IsNullLiteral(binaryExpression.Left);
        var rightIsNull = IsNullLiteral(binaryExpression.Right);
        
        if (!leftIsNull && !rightIsNull)
            return;

        // Get the non-null side
        var nonNullSide = leftIsNull ? binaryExpression.Right : binaryExpression.Left;
        
        // Check if the non-null side is a nullable type or reference type
        var typeInfo = context.SemanticModel.GetTypeInfo(nonNullSide);
        if (typeInfo.Type == null)
            return;

        // Check if it's a nullable value type or reference type
        bool isNullableType = typeInfo.Type.CanBeReferencedByName && 
                              (typeInfo.Type.IsReferenceType || 
                               (typeInfo.Type.IsValueType && typeInfo.Type.Name == "Nullable"));

        if (isNullableType)
        {
            var diagnostic = Diagnostic.Create(
                UseIsPatternForNullCheck,
                binaryExpression.GetLocation(),
                messageArgs: null);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullLiteral(SyntaxNode node)
    {
        return node is LiteralExpressionSyntax literal &&
               literal.Token.IsKind(SyntaxKind.NullKeyword);
    }
}