using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.DebugDisplay;
using ZoneRV.Core.Models.Sales;
using ZoneRV.OptionalProperties.Attributes;

namespace ZoneRV.Analyzer.Tests.DebugDisplayTests;

public class DebugDisplayWithOptionalFieldTests
{
    [Fact]
    public async Task OptionalFieldCauseError()
    {
        const string text = 
"""
using System.Diagnostics;
using ZoneRV.OptionalProperties.Attributes;

namespace ZoneRV.Analyzer.Tests.OptionalFields;

[DebuggerDisplay("{Id2} {{|#0:Id|#0}} {Id2}")]
public class Class1
{
    [OptionalProperty]
    public string Id { get; set; }

    public string Id2 { get; set; }
};

[DebuggerDisplay("{Id}")]
public class Class2
{
    public string Id { get; set; }
};

[DebuggerDisplay("{{|#1:Id|#1}.Length}")]
public class Class3
{
    [OptionalProperty]
    public string Id { get; set; }
};

""";

        var expected1 = new DiagnosticResult("ZRV0007", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' should not be used in DebugDisplay as it is optional")
            .WithArguments("Id");

        var expected2 = new DiagnosticResult("ZRV0007", DiagnosticSeverity.Error)
            .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' should not be used in DebugDisplay as it is optional")
            .WithArguments("Id");
        
        await new CSharpAnalyzerTest<PreventOptionalFieldsAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text  },
                    ExpectedDiagnostics = { expected1, expected2 },
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                    AdditionalReferences = { 
                        MetadataReference.CreateFromFile(typeof(Card).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalPropertyAttribute).Assembly.Location),
                    }
                }
            }
            .RunAsync();
    }
}