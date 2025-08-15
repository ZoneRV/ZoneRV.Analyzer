using System;
using System.Collections.Generic;
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

public class ForeachNameTests
{
    [Fact]
    public async Task CheckForeachParameterNames()
    {
        const string text = @"
public class SalesOrderRequestOptions { }

public class TestClass
{
    public void Test()
    {
        System.Collections.Generic.List<SalesOrderRequestOptions> options = [];

        foreach (var {|#0:filter|#0} in options)
        {
            
        }
    }
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
}