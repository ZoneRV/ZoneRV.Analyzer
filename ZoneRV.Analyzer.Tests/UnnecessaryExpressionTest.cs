using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.OptionalField;
using ZoneRV.Client.Models;
using ZoneRV.Core.Models.Sales;

namespace ZoneRV.Analyzer.Tests;

public class UnnecessaryExpressionTest
{
    [Fact]
    public async Task CheckNoFalsePositives()
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
        var options = new OptionalFieldCollection()
            .WithPropertiesFromType<SalesOrder>(x => x.Stats, x => x.Model.Line, x => x.LocationInfo.CurrentLocation.Location);
    }
}";
        
        await new CSharpAnalyzerTest<UnnecessaryExpressionAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text },
                    ExpectedDiagnostics = { },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalFieldCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
            .RunAsync();
    }
    
    [Fact]
    public async Task CheckWholeExpressionIsUnnecessary()
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
        var options = new OptionalFieldCollection()
            .WithPropertiesFromType<SalesOrder>({|#0:x => x.Id|#0}, x => x.Stats, {|#1:x => x.Model.Id|#1});
    }
}";
        
        var expected1 = new DiagnosticResult("ZRV0004", DiagnosticSeverity.Info)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0004);
        
        var expected2 = new DiagnosticResult("ZRV0004", DiagnosticSeverity.Info)
            .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0004);
        
        await new CSharpAnalyzerTest<UnnecessaryExpressionAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text },
                    ExpectedDiagnostics = { expected1, expected2},
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalFieldCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
            .RunAsync();
    }
    
    [Fact]
    public async Task CheckForTrailingUnnecessary()
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
        var options = new OptionalFieldCollection()
            .WithPropertiesFromType<SalesOrder>(x => x.Model.Line{|#0:.Id|#0}, x => x.LocationInfo.CurrentLocation.Location{|#1:.Name.Length|#1});
    }
}";
        
        var expected1 = new DiagnosticResult("ZRV0004", DiagnosticSeverity.Info)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0004);
        
        var expected2 = new DiagnosticResult("ZRV0004", DiagnosticSeverity.Info)
            .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0004);
        
        await new CSharpAnalyzerTest<UnnecessaryExpressionAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text },
                    ExpectedDiagnostics = { expected1, expected2},
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(SalesOrder).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalFieldCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
            .RunAsync();
    }
}