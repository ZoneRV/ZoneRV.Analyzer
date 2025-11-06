using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.HubSpot;
using ZoneRV.HubSpot.Models.Deal;

namespace ZoneRV.Analyzer.Tests.HubSpotTests;

public class ObjectNameMissingTests
{

    [Fact]
    public async Task ObjectNameMissingAnalyzer_NoError_ClassWithAttribute()
    {
        const string text = @"
using ZoneRV.HubSpot.Models;
using ZoneRV.HubSpot.Attributes;

[ObjectName(""Totally legit name"")]
public class MyCompanyClass : HubSpotEntityBase {}
";

        await new CSharpAnalyzerTest<ObjectNameMissingAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(Deal).Assembly.Location)
                    },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }

    [Fact]
    public async Task ObjectNameMissingAnalyzer_FindsError_BaseClass()
    {
        const string text = @"
using ZoneRV.HubSpot.Models;

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
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(Deal).Assembly.Location)
                    },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }

    [Fact]
    public async Task ObjectNameMissingAnalyzer_FindsError_IProperties()
    {
        const string text = @"
using ZoneRV.HubSpot.Models;

public class {|#0:MyCompanyClass|#0} : IProperties {}
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
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(Deal).Assembly.Location)
                    },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }

    [Fact]
    public async Task ObjectNameMissingAnalyzer_NoError_OnAbstractClass()
    {
        const string text = @"
using ZoneRV.HubSpot.Models;

public abstract class ComplexHubSpotBase : HubSpotEntityBase {}
";

        await new CSharpAnalyzerTest<ObjectNameMissingAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = { },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(Deal).Assembly.Location)
                    },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }

    [Fact]
    public async Task ObjectNameMissingAnalyzer_FindsError_ThroughInheritedClass()
    {
        const string text = @"
using ZoneRV.HubSpot.Models;

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
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(Deal).Assembly.Location)
                    },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                }
            }
           .RunAsync();
    }
}