using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.HubSpot;

namespace ZoneRV.Analyzer.Tests.HubSpotTests;

public class ObjectNameMissingTests
{

    [Fact]
    public async Task ObjectNameMissingAnalyzer_NoError_ClassWithAttribute()
    {
        const string text = @"
using System;

public class ObjectNameAttribute : Attribute {}

public abstract class HubSpotEntityBase {}

[ObjectName]
public class MyCompanyClass : HubSpotEntityBase {}
";

        await new CSharpAnalyzerTest<ObjectNameMissingAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }

    [Fact]
    public async Task ObjectNameMissingAnalyzer_FindsError_BaseClass()
    {
        const string text = @"
public abstract class HubSpotEntityBase {}

public class {|#0:MyCompanyClass|#0} : HubSpotEntityBase {}
";

        var expected = new DiagnosticResult("ZRVHS03", DiagnosticSeverity.Error)
                      .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRVHS03Title);

        await new CSharpAnalyzerTest<ObjectNameMissingAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { expected },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }

    [Fact]
    public async Task ObjectNameMissingAnalyzer_NoError_OnAbstractClass()
    {
        const string text = @"
public abstract class HubSpotEntityBase {}

public abstract class ComplexHubSpotBase : HubSpotEntityBase {}
";

        await new CSharpAnalyzerTest<ObjectNameMissingAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }

    [Fact]
    public async Task ObjectNameMissingAnalyzer_FindsError_ThroughInheritedClass()
    {
        const string text = @"
public abstract class HubSpotEntityBase {}

public abstract class ComplexHubSpotBase : HubSpotEntityBase {}

public class {|#0:MyCompanyClass|#0} : ComplexHubSpotBase {}
";

        var expected = new DiagnosticResult("ZRVHS03", DiagnosticSeverity.Error)
                      .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRVHS03Title);

        await new CSharpAnalyzerTest<ObjectNameMissingAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { expected },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }
}