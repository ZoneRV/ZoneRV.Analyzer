using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.DebugDisplay;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DebugDisplayMissingAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "ZRV0001",
        Resources.ZRV0001Title,
        Resources.ZRV0001MessageFormat,
        category: "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Resources.ZRV0001Description
    );
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
    
    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        // Subscribe to semantic (compile time) action invocation, e.g. method invocation.
        context.RegisterSyntaxNodeAction(AnalyzeOperation, SyntaxKind.ClassDeclaration);

        // Check other 'context.Register...' methods that might be helpful for your purposes.
    }

    private void AnalyzeOperation(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        // Check if the node is a class declaration
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var name = Utils.GetNamespaceOfClass(classDeclaration);

        if (name == null || !name.ToLower().Contains("models"))
        {
            // The class is not in the 'Models' namespace
            return;
        }

        // Check if the class already has a DebuggerDisplayAttribute
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null)
        {
            return;
        }

        var hasDebuggerDisplay = classSymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == "DebuggerDisplayAttribute");

        if (!hasDebuggerDisplay)
        {
            // The class is missing the DebuggerDisplayAttribute
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}