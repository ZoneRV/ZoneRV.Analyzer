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

public class AsyncNameTests
{
    [Fact]
    public async Task CheckVariableNames()
    {
        const string text = @"
using System.Threading.Tasks;

public class TestClass
{
    async Task {|#0:TestMethod|#0}()
    {
        await Task.Delay(100);
    }

    async Task TestMethodAsync()
    {
        await Task.Delay(100);
    }
}";
        
        var expected = new DiagnosticResult("ZRV0009", DiagnosticSeverity.Warning)
                      .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRV0009Title);

        await new CSharpAnalyzerTest<PoorNameAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { expected },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
           .RunAsync();
    }
}