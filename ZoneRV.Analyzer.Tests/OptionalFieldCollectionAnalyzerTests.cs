using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using ZoneRV.Client.Models;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.OptionalField;
using ZoneRV.Core.Models.Sales;

namespace ZoneRV.Analyzer.Tests;

public class OptionalFieldCollectionAnalyzerTests
{
    [Fact]
    public async Task OptionFieldCollectionContainsBadExpression()
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
            .WithPropertiesFromType<SalesOrder>(
                            x => x.JobCards, 
                            {|#0:x => x.Cards.First().Checklists|#0}, 
                            x => x.Stats, 
                            {|#1:x => x.Model.Line.Models.Last()|#1});
    }
}";
        
        var expected1 = new DiagnosticResult("ZRV0003", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' is not a valid property expression to create an optional field")
            .WithArguments("x => x.Cards.First().Checklists");
        
        var expected2 = new DiagnosticResult("ZRV0003", DiagnosticSeverity.Error)
            .WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat("'{0}' is not a valid property expression to create an optional field")
            .WithArguments("x => x.Model.Line.Models.Last()");
        
        await new CSharpAnalyzerTest<InvalidOptionalFieldExpressionAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text  },
                    ExpectedDiagnostics = { expected1, expected2 },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(Card).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalFieldCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
            .RunAsync();
    }
    
    [Fact]
    public async Task OptionFieldCollectionNoWarning()
    {
        const string text = @"
using ZoneRV.Client.Models;
using ZoneRV.Core.Models;
using ZoneRV.Core.Models.Sales;

namespace ZoneRV.Analyzer.Tests.OptionalFields;

public class Bar
{
    public void Main()
    {
        var options = new OptionalFieldCollection().WithPropertiesFromType<SalesOrder>(x => x.LocationInfo.CurrentLocation.Location, x => x.JobCards);
    }
}";
        
        await new CSharpAnalyzerTest<InvalidOptionalFieldExpressionAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { text  },
                    ExpectedDiagnostics = { },
                    AdditionalReferences =
                    {
                        MetadataReference.CreateFromFile(typeof(Card).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(OptionalFieldCollection).Assembly.Location)
                    },
                    
                    ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                }
            }
            .RunAsync();
    }
}