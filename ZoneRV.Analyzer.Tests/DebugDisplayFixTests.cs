using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    ZoneRV.Analyzer.DebugDisplay.DebugDisplayMissingAnalyzer,
    ZoneRV.Analyzer.DebugDisplay.AddDebugDisplayFixProvider>;

namespace ZoneRV.Analyzer.Tests;

public class DebugDisplayFixTests
{
    [Fact]
    public async Task FixAddsDisplayAttribute()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
@"namespace Models
{
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
@"using System.Diagnostics;

namespace Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
            [
                new DiagnosticResult("ZRV0001",
                    DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                    .WithArguments("TestClass")
            ], 
            FixedCode);
    }
    
    [Fact]
    public async Task FixAddsDisplayAttributeWithExistingUsings()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"using System;

namespace Models
{
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"using System;
using System.Diagnostics;

namespace Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
            [
                new DiagnosticResult("ZRV0001",
                        DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                    .WithArguments("TestClass")
            ], 
            FixedCode);
    }
    
    [Fact]
    public async Task FixAddsDisplayAttributeWithXmlDocs()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
@"namespace Models
{
    /// <summary>
    /// AAAAA
    /// </summary>
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
@"using System.Diagnostics;

namespace Models
{
    /// <summary>
    /// AAAAA
    /// </summary>
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
            [
                new DiagnosticResult("ZRV0001",
                    DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                    .WithArguments("TestClass")
            ], 
            FixedCode);
    }
    
    [Fact]
    public async Task FixAddsDisplayAttributePreserveExistingAttributes()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"using System;

namespace Models
{
    [Serializable]
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"using System;
using System.Diagnostics;

namespace Models
{
    [Serializable]
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
            [
                new DiagnosticResult("ZRV0001",
                        DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                    .WithArguments("TestClass")
            ], 
            FixedCode);
    }
    
    [Fact]
    public async Task FixAddsDisplayAttributeMultiple()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"namespace Models
{
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }

    public class {|#1:TestClass2|#1}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"using System.Diagnostics;

namespace Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }

    [DebuggerDisplay("""")]
    public class TestClass2
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
            [
                new DiagnosticResult("ZRV0001",
                        DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                    .WithArguments("TestClass"),
                new DiagnosticResult("ZRV0001",
                        DiagnosticSeverity.Warning)
                    .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
                    .WithArguments("TestClass2")
            ], 
            FixedCode);
    }
    
    [Fact]
    public async Task FixAddsDisplayAttributeMultipleButOneIsGood()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"using System.Diagnostics;
namespace Models
{
    [DebuggerDisplay("""")]
    public class {|#0:TestClass|#0}
    {
        public int Id { get; set; }
    }

    public class {|#1:TestClass2|#1}
    {
        public int Id { get; set; }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"using System.Diagnostics;
namespace Models
{
    [DebuggerDisplay("""")]
    public class TestClass
    {
        public int Id { get; set; }
    }

    [DebuggerDisplay("""")]
    public class TestClass2
    {
        public int Id { get; set; }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
            [
                new DiagnosticResult("ZRV0001",
                        DiagnosticSeverity.Warning)
                    .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
                    .WithArguments("TestClass2")
            ], 
            FixedCode);
    }
    
    [Fact]
    public async Task FixIsNotAppliedIfDebuggerDisplayExists()
    {
        // Input code where DebuggerDisplay is already present
        const string TestCode = @"
        using System.Diagnostics;

        namespace Models
        {
            [DebuggerDisplay(""Id = {Id}"")]
            public class TestClass
            {
                public int Id { get; set; }
            }
        }";

        // Expected code (should remain unchanged because the attribute already exists)
        const string FixedCode = TestCode;

        // Verify that no additional attribute is added
        await Verifier.VerifyCodeFixAsync(TestCode, FixedCode);
    }
    
    [Fact]
    public async Task NameSpaceIsNotModels()
    {
        // Input code where DebuggerDisplay is already present
        const string TestCode = @"
        namespace NoDisplayPlz
        {
            public class TestClass
            {
                public int Id { get; set; }
            }
        }";

        // Expected code (should remain unchanged because the attribute already exists)
        const string FixedCode = TestCode;

        // Verify that no additional attribute is added
        await Verifier.VerifyCodeFixAsync(TestCode, FixedCode);
    }
}