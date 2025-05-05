using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Client.Models;
using ZoneRV.Core.Models.Sales;

namespace ZoneRV.Analyzer.Tests;

public class PoorNameAnalyzerTests
{
    [Fact]
    public async Task CorrectVariableNameForPerson_ShouldNotTriggerWarning()
    {
        const string text = @"
public class SalesOrderRequestOptions { }

public class TestClass
{
    void TestMethod()
    {
        var {|#0:filterOptions|#0} = new SalesOrderRequestOptions();
        var requestOptions = new SalesOrderRequestOptions();
    }
}";
        
        var expected = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0006MessageFormat)
            .WithArguments("SalesOrderRequestOptions", "filterOptions");

        await new CSharpAnalyzerTest<PoorNameAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text },
                    ExpectedDiagnostics = { expected },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalFieldCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
            .RunAsync();
    }
}