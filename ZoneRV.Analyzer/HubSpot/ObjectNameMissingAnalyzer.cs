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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeOperation, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeOperation(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // ignore interfaces
        if(context.Node is InterfaceDeclarationSyntax)
            return;

        // Ignore abstract classes
        if(classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
            return;

        var classSymbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, classDeclaration);
        if (classSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        if (!InheritsFromRequiredClasses(namedTypeSymbol))
        {
            return;
        }

        var hasObjectNameAttribute = classSymbol
                                    .GetAttributes()
                                    .Any(attr => attr.AttributeClass?.Name is "ObjectNameAttribute");

        if (!hasObjectNameAttribute)
        {
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool InheritsFromRequiredClasses(INamedTypeSymbol classSymbol)
    {
        var currentType = classSymbol.BaseType;

        // Check base types
        while (currentType != null)
        {
            if (currentType.Name is "HubSpotEntityBase")
            {

                var baseNamespace = currentType.ContainingNamespace?.ToDisplayString();
                if (!string.IsNullOrEmpty(baseNamespace) && baseNamespace.StartsWith("ZoneRV.HubSpot.Models"))
                {
                    return true;
                }

                return false;
            }
            currentType = currentType.BaseType;
        }

        // Check interfaces because Roslyn is a pain to program
        foreach (var interfaceType in classSymbol.AllInterfaces)
        {
            if (interfaceType.Name is "IProperties")
            {
                var interfaceNamespace = interfaceType.ContainingNamespace?.ToDisplayString();
                if (!string.IsNullOrEmpty(interfaceNamespace) && interfaceNamespace.StartsWith("ZoneRV.HubSpot.Models"))
                {
                    return true;
                }
            }
        }

        return false;
    }

}