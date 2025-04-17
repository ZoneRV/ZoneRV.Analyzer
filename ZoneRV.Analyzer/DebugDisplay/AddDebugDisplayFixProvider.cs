using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ZoneRV.Analyzer.DebugDisplay;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddDebugDisplayFixProvider))]
public class AddDebugDisplayFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds 
        => ImmutableArray.Create("ZRV0001");

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;
    
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create(
                Resources.ZRV0001CodeAction,
                c => AddDebuggerDisplayAsync(context.Document, classDeclaration, c),
                equivalenceKey: Resources.ZRV0001Title),
            diagnostic);
    }
    
    private async Task<Document> AddDebuggerDisplayAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
    {
        // Check if the class already has the DebuggerDisplay attribute
        var existingAttributes = classDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "DebuggerDisplay");

        // If the attribute already exists, do nothing
        if (existingAttributes)
        {
            return document;
        }
        
        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("DebuggerDisplay"))
                    .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(string.Empty))))))));

        // Preserve any existing trivia (e.g., XML documentation, comments, whitespace)
        var leadingTrivia = classDeclaration.GetLeadingTrivia();
        var trailingTrivia = classDeclaration.GetTrailingTrivia();

        // Attach the attribute after the XML documentation
        var newClassDeclaration = classDeclaration
            .WithoutTrivia()
            .WithTrailingTrivia(trailingTrivia)
            .AddAttributeLists(attributeList) // Add the DebuggerDisplay attribute
            .WithLeadingTrivia(SyntaxFactory.TriviaList(leadingTrivia)); // Add other trivia like whitespace

        // Replace the old class declaration with the new one
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return await AddUsingIfMissingAsync(document.WithSyntaxRoot(newRoot), cancellationToken);
    }
    
    private async Task<Document> AddUsingIfMissingAsync(Document document, CancellationToken cancellationToken)
    {
        // Parse the document's syntax root
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var compilationUnit = root as CompilationUnitSyntax;

        // Check if the using directive already exists
        var hasUsingDirective = compilationUnit?.Usings
            .Any(usingDirective => usingDirective.Name.ToString() == "System.Diagnostics") == true;

        if (hasUsingDirective)
        {
            return document; // Return the original document if the using directive exists
        }

        bool addExtraLine = compilationUnit?.Usings.Count == 0;
    
        // Create a new using directive
        UsingDirectiveSyntax newUsing;
            
        if(addExtraLine)
            newUsing =  SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics")).NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)
                .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
        
        else
            newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics")).NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        // Add the using directive to the syntax tree
        var newCompilationUnit = compilationUnit?.AddUsings(newUsing);

        // Return a new document with the updated root
        return document.WithSyntaxRoot(newCompilationUnit);
    }
}