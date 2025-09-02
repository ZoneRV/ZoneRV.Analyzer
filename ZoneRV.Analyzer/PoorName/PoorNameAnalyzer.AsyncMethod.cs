using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZoneRV.Analyzer.PoorName;

public partial class PoorNameAnalyzer
{
    private void MethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclarator = (MethodDeclarationSyntax)context.Node;

        // Skip xUnit/NUnit tests (including common NUnit setup/fixture methods)
        if (IsXUnitOrNUnitTestMethod(methodDeclarator, context.SemanticModel))
            return;

        // Syntactic check: looks for the 'async' modifier directly
        var isAsyncBySyntax = methodDeclarator.Modifiers.Any(SyntaxKind.AsyncKeyword);

        // Semantic check: asks the symbol if it's async (requires SemanticModel)
        var symbol          = context.SemanticModel.GetDeclaredSymbol(methodDeclarator);
        var isAsyncBySymbol = symbol?.IsAsync == true;

        var isAsync = isAsyncBySyntax || isAsyncBySymbol;

        if (!methodDeclarator.Identifier.ValueText.EndsWith("async", StringComparison.OrdinalIgnoreCase) && isAsync)
        {
            var diagnostic = Diagnostic.Create(AsyncRule, methodDeclarator.Identifier.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsXUnitOrNUnitTestMethod(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        // Check method-level attributes (xUnit: [Fact], [Theory]; NUnit: [Test], [TestCase], etc.)
        if (HasAnyTestAttribute(method.AttributeLists, semanticModel))
            return true;

        // Check containing class for NUnit fixtures or test-level indicators
        if (method.Parent is TypeDeclarationSyntax typeDecl)
        {
            if (HasAnyTestAttribute(typeDecl.AttributeLists, semanticModel))
                return true;
        }

        return false;
    }

    private static bool HasAnyTestAttribute(SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel)
    {
        foreach (var list in attributeLists)
        {
            foreach (var attr in list.Attributes)
            {
                var typeInfo = semanticModel.GetTypeInfo(attr);
                var attrType = typeInfo.Type;

                // Fallback to simple identifier if semantic model cannot resolve
                var simpleName = attr.Name switch
                {
                    IdentifierNameSyntax id => id.Identifier.ValueText,
                    QualifiedNameSyntax q   => q.Right.Identifier.ValueText,
                    _                       => attr.Name.ToString()
                };

                // When symbol is resolved, use namespace + type name checks
                if (attrType is INamedTypeSymbol named)
                {
                    var ns = named.ContainingNamespace?.ToDisplayString() ?? string.Empty;
                    var name = named.Name;

                    if (IsXunitAttribute(ns, name) || IsNUnitAttribute(ns, name))
                        return true;
                }
                else
                {
                    // Best-effort textual fallback
                    if (IsXunitAttribute("", simpleName) || IsNUnitAttribute("", simpleName))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool IsXunitAttribute(string ns, string name)
    {
        // xUnit attributes commonly used on test methods
        // Names may appear with or without the "Attribute" suffix depending on resolution
        if (!string.IsNullOrEmpty(ns) && !ns.Equals("Xunit", StringComparison.Ordinal))
            return false;

        return MatchesAttr(name, "Fact")
            || MatchesAttr(name, "Theory")
            || MatchesAttr(name, "ClassData")
            || MatchesAttr(name, "MemberData")
            || MatchesAttr(name, "InlineData");
    }

    private static bool IsNUnitAttribute(string ns, string name)
    {
        // NUnit attributes for test methods, cases, fixtures, and setups/teardowns
        if (!string.IsNullOrEmpty(ns) && !ns.Equals("NUnit.Framework", StringComparison.Ordinal))
            return false;

        return MatchesAttr(name, "Test")
            || MatchesAttr(name, "TestCase")
            || MatchesAttr(name, "TestCaseSource")
            || MatchesAttr(name, "TestFixture")
            || MatchesAttr(name, "TestFixtureSource")
            || MatchesAttr(name, "TestOf")
            || MatchesAttr(name, "SetUp")
            || MatchesAttr(name, "TearDown")
            || MatchesAttr(name, "OneTimeSetUp")
            || MatchesAttr(name, "OneTimeTearDown");
    }

    private static bool MatchesAttr(string actualName, string baseName)
    {
        // Handles both "Fact" and "FactAttribute" forms
        return string.Equals(actualName, baseName, StringComparison.Ordinal)
            || string.Equals(actualName, baseName + "Attribute", StringComparison.Ordinal);
    }
}