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
    public async Task CheckVariableNames()
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
    
    [Fact]
    public async Task CheckMemberNames()
    {
        const string text = @"
public class SalesOrderRequestOptions { }

public class TestClass
{
    SalesOrderRequestOptions {|#0:filterOptions|#0} { get; set; }
    SalesOrderRequestOptions requestOptions { get; set; }

    SalesOrderRequestOptions {|#1:filterOptions1|#1};
    SalesOrderRequestOptions requestOptions1;

    SalesOrderRequestOptions? {|#2:filterOptions2|#2} { get; set; }
    SalesOrderRequestOptions? requestOptions2 { get; set; }
                             
    SalesOrderRequestOptions? {|#3:filterOptions3|#3};
    SalesOrderRequestOptions? requestOptions3;
}";
        
        var expected = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0006MessageFormat)
            .WithArguments("SalesOrderRequestOptions", "filterOptions");
        
        var expected2 = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
            .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0006MessageFormat)
            .WithArguments("SalesOrderRequestOptions", "filterOptions1");
        
        var expected3 = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
            .WithLocation(2, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0006MessageFormat)
            .WithArguments("SalesOrderRequestOptions", "filterOptions2");
        
        var expected4 = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
            .WithLocation(3, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0006MessageFormat)
            .WithArguments("SalesOrderRequestOptions", "filterOptions3");

        await new CSharpAnalyzerTest<PoorNameAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text },
                    ExpectedDiagnostics = { expected, expected2, expected3, expected4 },
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