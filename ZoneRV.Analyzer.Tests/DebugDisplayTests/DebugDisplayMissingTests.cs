using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    ZoneRV.Analyzer.DebugDisplay.DebugDisplayMissingAnalyzer>;

namespace ZoneRV.Analyzer.Tests.DebugDisplayTests;

public class DebugDisplayMissingTests
{
    [Fact]
    public async Task ClasWithNoDebugDisplayFileNamespace_AlertDiagnostic()
    {
        const string text = @"
                            namespace ZoneRV.Models;
                            public class {|#0:Class1|#0}
                            {
                            }
                            ";

        var expected = new DiagnosticResult("ZRV0001", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' should have a DebugDisplay")
            .WithArguments("Class1");
        
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
    
    [Fact]
    public async Task ClasWithNoDebugDisplayScopeNamespace_AlertDiagnostic()
    {
        const string text = @"
                            namespace ZoneRV.Models
                            {
                            public class {|#0:Class1|#0}
                            {
                            }
                            }
                            ";

        var expected = new DiagnosticResult("ZRV0001", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithArguments("Class1");
        
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
    
    [Fact]
    public async Task IgnoreInterfaces()
    {
        const string text = @"
                            namespace ZoneRV.Models
                            {
                            public interface IClass1
                            {
                            }
                            }
                            ";
        
        await Verifier.VerifyAnalyzerAsync(text);
    }
    
    [Fact]
    public async Task IgnoreAbstractClass()
    {
        const string text = @"
                            namespace ZoneRV.Models
                            {
                            public abstract class Class1
                            {
                            }
                            }
                            ";
        
        await Verifier.VerifyAnalyzerAsync(text);
    }
}