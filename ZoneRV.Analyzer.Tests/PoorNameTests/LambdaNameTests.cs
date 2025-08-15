using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.PoorName;
using ZoneRV.Client.Models;
using ZoneRV.Core.Models.Sales;

namespace ZoneRV.Analyzer.Tests.PoorNameTests;

public class LambdaNameTests
{
    [Fact]
    public async Task CheckLambdaParameterNames()
    {
        const string text = @"
public class SalesOrderRequestOptions { }

public class TestClass
{
    public System.Func<SalesOrderRequestOptions, int> Func = {|#0:filter|#0} => 15;
}";
        
        var expected = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
                      .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRV0006MessageFormat)
                      .WithArguments("SalesOrderRequestOptions", "filter");

        await new CSharpAnalyzerTest<PoorNameAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { expected },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalPropertyCollection).Assembly.Location),
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
           .RunAsync();
    }
    
    [Fact]
    public async Task CheckParenthesizedLambdaParameterNames()
    {
        const string text = @"
public class SalesOrderRequestOptions { }

public class TestClass
{
    public System.Func<SalesOrderRequestOptions, SalesOrderRequestOptions, int> Func = ({|#0:filter|#0}, {|#1:filter2|#1}) => 15;
}";
        
        var expected = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
                      .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRV0006MessageFormat)
                      .WithArguments("SalesOrderRequestOptions", "filter");
        
        var expected2 = new DiagnosticResult("ZRV0006", DiagnosticSeverity.Warning)
                      .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRV0006MessageFormat)
                      .WithArguments("SalesOrderRequestOptions", "filter");

        await new CSharpAnalyzerTest<PoorNameAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { expected, expected2 },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalPropertyCollection).Assembly.Location),
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
           .RunAsync();
    }
}