using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ZoneRV.Analyzer;

public static class Utils
{
    public static string? GetNamespaceOfClass(ClassDeclarationSyntax classDeclaration)
    {
        // Declare a variable to store the namespace as you traverse the tree
        string? namespaceName = null;

        // Traverse upward to locate the namespace declaration
        var parent = classDeclaration.Parent;

        while (parent != null)
        {
            if (parent is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                // Prepend the namespace name to build nested namespaces
                namespaceName = namespaceDeclaration.Name.ToString() +
                                (namespaceName == null ? string.Empty : "." + namespaceName);
            }
            else if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
            {
                // Handle file-scoped namespaces
                namespaceName = fileScopedNamespace.Name.ToString() +
                                (namespaceName == null ? string.Empty : "." + namespaceName);
            }

            // Continue traversing upward
            parent = parent.Parent;
        }

        return namespaceName;
    }
}