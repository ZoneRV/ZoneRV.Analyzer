using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    ZoneRV.Analyzer.DebugDisplay.EmptyDebugDisplayAnalyzer>;

namespace ZoneRV.Analyzer.Tests.DebugDisplayTests;

public class DebugEmptyTests
{
    [Fact]
    public async Task ValueIsEmpty_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""""|#0})]
                            public class Class1{}
                            ";

        var expected = new DiagnosticResult("ZRV0002", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Display argument is invalid");
        
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
    
    [Fact]
    public async Task ValueIsWhiteSpace_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""     ""|#0})]
                            public class Class1{}";

        var expected = new DiagnosticResult("ZRV0002", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Display argument is invalid");
        
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
    
    [Fact]
    public async Task ValueDoesNotHaveBraces_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""Thisisuseless""|#0})]
                            public class Class1
                            {
                                public int Id { get; set; }
                            };";

        var expected = new DiagnosticResult("ZRV0002", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Display argument is invalid");
        
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
    
    [Fact]
    public async Task ValueHasOneBrace_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""Thisisuseless}""|#0})]
                            public class Class1
                            {
                                public int Id { get; set; }
                            };";

        var expected = new DiagnosticResult("ZRV0002", DiagnosticSeverity.Warning)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("Debug Display argument is invalid");
        
        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
    
    [Fact]
    public async Task NoIssue_AlertDiagnostic()
    {
        const string text = @"using System.Diagnostics;
                            [DebuggerDisplay({|#0:""{ThisIsUseful}""|#0})]
                            public class Class1
                            {
                                public int Id { get; set; }
                            };";
        
        await Verifier.VerifyAnalyzerAsync(text, []);
    }
}