using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.OptionalField;
using ZoneRV.Client.Models;
using ZoneRV.Core.Attributes;
using ZoneRV.Core.Models.Location;
using ZoneRV.Core.Models.Production;
using ZoneRV.Core.Models.Sales;

namespace ZoneRV.Analyzer.Tests;

public class UnnecessaryExpressionTest
{
    [Fact]
    public async Task CheckForOptionalWarnings()
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
            .WithPropertiesFromType<SalesOrder>({|#0:x => x.Id|#0}, {|#1:x => x.Model.Id|#1}, x => x.Model.Line{|#2:.Id|#2}, x => x.LocationInfo.CurrentLocation.Location);
    }
}";
        
        var expected1 = new DiagnosticResult("ZRV0004", DiagnosticSeverity.Hidden)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0004);
        
        var expected2 = new DiagnosticResult("ZRV0004", DiagnosticSeverity.Hidden)
            .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0004);
        
        var expected3 = new DiagnosticResult("ZRV0004", DiagnosticSeverity.Hidden)
            .WithLocation(2, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRV0004);
        
        await new CSharpAnalyzerTest<UnnecessaryExpressionAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text },
                    ExpectedDiagnostics = { expected1, expected2, expected3},
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