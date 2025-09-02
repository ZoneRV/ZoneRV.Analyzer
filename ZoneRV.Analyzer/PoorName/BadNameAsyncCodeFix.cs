using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace ZoneRV.Analyzer.PoorName;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PoorNameAnalyzer)), Shared]
public class BadNameAsyncCodeFix : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["ZRV0009"];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var cancellationToken = context.CancellationToken;
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);
            var methodDecl = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDecl is null)
            {
                continue;
            }

            var currentName = methodDecl.Identifier.ValueText;
            var newName = currentName.EndsWith("Async") ? currentName : currentName + "Async";

            var title = "Append 'Async' suffix";
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    ct => RenameMethodAsync(document, methodDecl, newName, ct),
                    equivalenceKey: "AppendAsyncSuffix"),
                diagnostic);
        }
    }

    private static async Task<Solution> RenameMethodAsync(Document document, MethodDeclarationSyntax methodDecl, string newName, CancellationToken ct)
    {
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document.Project.Solution;
        }

        var symbol = semanticModel.GetDeclaredSymbol(methodDecl, ct);
        if (symbol is null)
        {
            return document.Project.Solution;
        }

        // Use workspace options to be broadly compatible across Roslyn versions.
        var solution = document.Project.Solution;
        
        return await Renamer.RenameSymbolAsync(solution, symbol, new SymbolRenameOptions(RenameOverloads: true), newName, ct).ConfigureAwait(false);
    }
}