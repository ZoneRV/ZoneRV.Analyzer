using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ZoneRV.Analyzer.NullEquality;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullCheckCodeFixProvider)), Shared]
public class NullCheckCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("ZRV0008");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
        if (diagnostic is null)
            return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var binaryExpression = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                                   .OfType<BinaryExpressionSyntax>().FirstOrDefault();

        if (binaryExpression is null)
            return;

        // Determine the replacement pattern
        var isEqualsExpression = binaryExpression.IsKind(SyntaxKind.EqualsExpression);
        var leftIsNull         = IsNullLiteral(binaryExpression.Left);
        var nonNullExpression  = leftIsNull ? binaryExpression.Right : binaryExpression.Left;
        
        var replacementText = isEqualsExpression ? "is null" : "is not null";
        var title           = $"Use '{replacementText}' pattern";

        var action = CodeAction.Create(
            title: title,
            createChangedDocument: c => ReplaceWithIsPattern(context.Document, root, binaryExpression, nonNullExpression, isEqualsExpression, c),
            equivalenceKey: title);

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> ReplaceWithIsPattern(
        Document               document,
        SyntaxNode             root,
        BinaryExpressionSyntax binaryExpression,
        ExpressionSyntax       nonNullExpression,
        bool                   isEqualsExpression,
        CancellationToken      cancellationToken)
    {
        // Create the is pattern expression
        var isPatternExpression = isEqualsExpression
            ? SyntaxFactory.IsPatternExpression(
                nonNullExpression,
                SyntaxFactory.ConstantPattern(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
            : SyntaxFactory.IsPatternExpression(
                nonNullExpression,
                SyntaxFactory.UnaryPattern(
                    SyntaxFactory.Token(SyntaxKind.NotKeyword),
                    SyntaxFactory.ConstantPattern(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))));

        // Preserve trivia from the original expression
        var newExpression = isPatternExpression
                           .WithLeadingTrivia(binaryExpression.GetLeadingTrivia())
                           .WithTrailingTrivia(binaryExpression.GetTrailingTrivia());

        // Replace the binary expression with the is pattern expression
        var newRoot = root.ReplaceNode(binaryExpression, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }

    private static bool IsNullLiteral(SyntaxNode node)
    {
        return node is LiteralExpressionSyntax literal &&
               literal.Token.IsKind(SyntaxKind.NullKeyword);
    }
}