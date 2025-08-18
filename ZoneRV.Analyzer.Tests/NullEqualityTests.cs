using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.NullEquality;
using ZoneRV.Analyzer.OptionalField;
using ZoneRV.Client.Models;
using ZoneRV.Core.Models.Sales;

using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    ZoneRV.Analyzer.NullEquality.NullCheckAnalyzer,
    ZoneRV.Analyzer.NullEquality.NullCheckCodeFixProvider>;

namespace ZoneRV.Analyzer.Tests;

public class NullEqualityTests
{
    [Fact]
    public async Task NoWarningUsingIsKeyword()
    {
        
        const string text = @"
using ZoneRV.Client.Models;
using ZoneRV.Core.Models;
using ZoneRV.Core.Models.Sales;
using System.Linq;

namespace ZoneRV.Analyzer.Tests.OptionalFields;

public class Bar
{
    public void Main()
    {
        OptionalPropertyCollection? options = null;

        if(options is null)
            options = new ();

        if(options is not null)
            options = null;
    }
}";
        
        await new CSharpAnalyzerTest<NullCheckAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = {  },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalPropertyCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
           .RunAsync();
    }
    
    [Fact]
    public async Task TestEqualsCheck()
    {
        const string text = @"
using ZoneRV.Client.Models;
using ZoneRV.Core.Models;
using ZoneRV.Core.Models.Sales;
using System.Linq;

namespace ZoneRV.Analyzer.Tests.OptionalFields;

public class Bar
{
    public void Main()
    {
        OptionalPropertyCollection? options = null;

        if({|#0:options == null|#0})
            options = new ();
    }
}";
        
        var expected1 = new DiagnosticResult("ZRV0008", DiagnosticSeverity.Warning)
                       .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                       .WithMessageFormat(Resources.ZRV0008MessageFormat);
        
        await new CSharpAnalyzerTest<NullCheckAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { expected1 },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalPropertyCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
           .RunAsync();
    }
    
    [Fact]
    public async Task TestNotEqualsCheck()
    {
        const string text = @"
using ZoneRV.Client.Models;
using ZoneRV.Core.Models;
using ZoneRV.Core.Models.Sales;
using System.Linq;

namespace ZoneRV.Analyzer.Tests.OptionalFields;

public class Bar
{
    public void Main()
    {
        OptionalPropertyCollection? options = null;

        if({|#0:options != null|#0})
            options = null;
    }
}";
        
        var expected1 = new DiagnosticResult("ZRV0008", DiagnosticSeverity.Warning)
                       .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                       .WithMessageFormat(Resources.ZRV0008MessageFormat);
        
        await new CSharpAnalyzerTest<NullCheckAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { expected1 },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalPropertyCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
           .RunAsync();
    }
    
    [Fact]
    public async Task EqualCodeFixTest()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"namespace Models
{
    public class TestClass
    {
        void Main()
        {
            TestClass? o = null;
            
            if ({|#0:o == null|#0})
                System.Console.WriteLine(""a"");
        }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"namespace Models
{
    public class TestClass
    {
        void Main()
        {
            TestClass? o = null;
            
            if (o is null)
                System.Console.WriteLine(""a"");
        }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
                                          [
                                              new DiagnosticResult("ZRV0008",
                                                                   DiagnosticSeverity.Warning)
                                                 .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                          ], 
                                          FixedCode);
    }
    
    [Fact]
    public async Task NotEqualCodeFixTest()
    {
        // Input code that will trigger the analyzer
        const string TestCode = 
            @"namespace Models
{
    public class TestClass
    {
        void Main()
        {
            TestClass? o = null;
            
            if ({|#0:o != null|#0})
                System.Console.WriteLine(""a"");
        }
    }
}";

        // Expected code after applying the CodeFixProvider
        const string FixedCode = 
            @"namespace Models
{
    public class TestClass
    {
        void Main()
        {
            TestClass? o = null;
            
            if (o is not null)
                System.Console.WriteLine(""a"");
        }
    }
}";

        // Verify the code fix
        await Verifier.VerifyCodeFixAsync(TestCode, 
                                          [
                                              new DiagnosticResult("ZRV0008",
                                                                   DiagnosticSeverity.Warning)
                                                 .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                          ], 
                                          FixedCode);
    }
}