using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.PoorName;
using ZoneRV.Client.Models;
using ZoneRV.Core.Models.Sales;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    ZoneRV.Analyzer.PoorName.PoorNameAnalyzer,
    ZoneRV.Analyzer.PoorName.BadNameAsyncCodeFix>;

namespace ZoneRV.Analyzer.Tests.PoorNameTests;

public class AsyncNameTests
{
    [Fact]
    public async Task IgnoreMain()
    {
        const string text = @"
using System.Threading.Tasks;

public class Program
{
    async Task Main()
    {
        await Task.Delay(100);
    }
}";
        

        await new CSharpAnalyzerTest<PoorNameAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = {},
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
           .RunAsync();
    }
    
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

    [Fact]
    public async Task CodeFixAppendsAsync()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"
using System.Threading.Tasks;

public class TestClass
{
    public async Task {|#0:task|#0}()
    {
        await Task.Delay(50);
    }
}
";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"
using System.Threading.Tasks;

public class TestClass
{
    public async Task taskAsync()
    {
        await Task.Delay(50);
    }
}
";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
                                          [
                                              new DiagnosticResult("ZRV0009",
                                                                   DiagnosticSeverity.Warning)
                                                 .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                          ], 
                                          FixedCode);
    }

    [Fact]
    public async Task CodeFixAppendsAsyncToOverrides()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"
using System.Threading.Tasks;

public abstract class TestClass
{
    public abstract Task task();
}

public class TestClass2 : TestClass
{
    public override async Task {|#0:task|#0}()
    {
        await Task.Delay(50);
    }
}
";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"
using System.Threading.Tasks;

public abstract class TestClass
{
    public abstract Task taskAsync();
}

public class TestClass2 : TestClass
{
    public override async Task taskAsync()
    {
        await Task.Delay(50);
    }
}
";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
                                          [
                                              new DiagnosticResult("ZRV0009",
                                                                   DiagnosticSeverity.Warning)
                                                 .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                          ], 
                                          FixedCode);
    }
}