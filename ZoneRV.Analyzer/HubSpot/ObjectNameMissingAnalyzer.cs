using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.HubSpot;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ObjectNameMissingAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "ZRVHS03",
        Resources.ZRVHS03Title,
        Resources.ZRVHS03Title,
        category: "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        // Subscribe to semantic (compile time) action invocation, e.g. method invocation.
        context.RegisterSyntaxNodeAction(AnalyzeOperation, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeOperation(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        // Check if the node is a class declaration
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // ignore interfaces
        if(context.Node is InterfaceDeclarationSyntax)
            return;

        // Ignore abstract classes
        if(classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
            return;

        // Check if the class already has a DebuggerDisplayAttribute
        var classSymbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, classDeclaration);
        if (classSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        // Check if class inherits from HubSpotEntityBase
        if (!InheritsFromHubSpotEntityBase(namedTypeSymbol))
        {
            return;
        }

        var hasObjectNameAttribute = classSymbol
                                    .GetAttributes()
                                    .Any(attr => attr.AttributeClass?.Name == "ObjectNameAttribute");

        if (!hasObjectNameAttribute)
        {
            // The class is missing the DebuggerDisplayAttribute
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    // Check if the class inherits from HubSpotEntityBase
    private bool InheritsFromHubSpotEntityBase(INamedTypeSymbol classSymbol)
    {
        var currentType = classSymbol.BaseType;

        while (currentType != null)
        {
            if (currentType.Name == "HubSpotEntityBase")
            {
                return true;
            }
            currentType = currentType.BaseType;
        }

        return false;
    }
}