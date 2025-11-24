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

namespace ZoneRV.Analyzer.HubSpot;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExpectConstCodeFix)), Shared]
public class ExpectConstCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["ZRVHS02"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var cancellationToken = context.CancellationToken;
        var document = context.Document;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var span = diagnostic.Location.SourceSpan;
            var node = root.FindNode(span);
            var argDecl = node.FirstAncestorOrSelf<ArgumentSyntax>();

            // We expect the node to be a literal expression
            if (argDecl is null)
                continue;

            if (!diagnostic.Properties.TryGetValue("constant", out var constantName))
                continue;

            var title = $"Replace with {constantName}";
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    ct => ReplaceLiteralWithConstant(document, argDecl, constantName!, ct),
                    equivalenceKey: "ReplaceLiteralWithConstant"),
                diagnostic);
        }
    }

    private static async Task<Document> ReplaceLiteralWithConstant(
        Document document,
        ArgumentSyntax argumentSyntax,
        string constantName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
            return document;

        // Find the specific literal within the argument if present
        var literalNode = argumentSyntax
            .DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .FirstOrDefault(lit => lit.Span.IntersectsWith(argumentSyntax.Span));

        // Build the replacement expression (DealProperties.AmountJson)
        var parts = constantName.Split('.');

        ExpressionSyntax newExpression =
            parts.Length == 2
                ? SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(parts[0]),
                    SyntaxFactory.IdentifierName(parts[1]))
                : SyntaxFactory.IdentifierName(constantName);

        newExpression = newExpression
            .WithLeadingTrivia(literalNode?.GetLeadingTrivia() ?? argumentSyntax.GetLeadingTrivia())
            .WithTrailingTrivia(literalNode?.GetTrailingTrivia() ?? argumentSyntax.GetTrailingTrivia());

        SyntaxNode newRoot;

        if (literalNode is not null)
        {
            // literal inside collection or array initializer
            newRoot = root.ReplaceNode(literalNode, newExpression);
        }
        else
        {
            // simple argument
            var newArg = argumentSyntax.WithExpression(newExpression);
            newRoot = root.ReplaceNode(argumentSyntax, newArg);
        }

        return document.WithSyntaxRoot(newRoot);
    }
}