using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.OptionalField;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidOptionalFieldExpressionAnalyzer: DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "ZRV0003",
        Resources.ZRV0003Title,
        Resources.ZRV0003MessageFormat,
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Resources.ZRV0003Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        // Get the invocation expression syntax node
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if the method name is "WithPropertiesFromType"
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text == "WithPropertiesFromType")
        {
            foreach (var expression in invocation.ArgumentList.Arguments.Select(x => x.Expression as LambdaExpressionSyntax))
            {
                // Validate the body of the lambda expression
                ValidateLambdaExpression(expression, context);
            }
        }
    }

    private static void ValidateLambdaExpression(LambdaExpressionSyntax lambdaExpression, SyntaxNodeAnalysisContext context)
    {
        if (lambdaExpression.Body is ExpressionSyntax body)
        {
            var decendants = body.DescendantNodes();
            // Check the body and ensure there are no InvocationExpressions
            var containsInvalidNodes = decendants
                                            .OfType<ArgumentListSyntax>() // Find any method calls in the body
                                            .Any();

            if (containsInvalidNodes)
            {
                // Report a diagnostic at the location of the lambda body
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    lambdaExpression.GetLocation(),
                    lambdaExpression.ToString()));
            }
        }
    }
}