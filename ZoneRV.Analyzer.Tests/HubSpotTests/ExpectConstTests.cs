using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using ZoneRV.Analyzer.HubSpot;
using ZoneRV.HubSpot.Models.Deal;

namespace ZoneRV.Analyzer.Tests.HubSpotTests;

public class ExpectConstTests
{
    [Fact]
    public async Task ExpectConstInArgumentsAnalyzer_NoError_WhenConstantIsntFromIProperty()
    {
        const string text = @"
using System.Threading.Tasks;
using ZoneRV.HubSpot.Models.Deal;
using ZoneRV.HubSpot.Models.Quote;
using ZoneRV.HubSpot.Client;

public class MyCompanyClass
{
    async Task Test()
    {
        var client = new HubSpotClient(""api"");

        var deal = await client.GetAsync<Deal>(0, properties: [HubSpotClient.DefaultUriBase]);
    }
}
";

        await new CSharpAnalyzerTest<ExpectConstInArgumentsAnalyzer, XUnitVerifier>
            {
                TestState =
                {
                    Sources             = { text },
                    ExpectedDiagnostics = {  },
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
    public async Task ExpectConstInArgumentsAnalyzer_FindsInvalidPropertyConstant_GenericMethod()
    {
        const string text = @"
using System.Threading.Tasks;
using ZoneRV.HubSpot.Models.Deal;
using ZoneRV.HubSpot.Models.Quote;
using ZoneRV.HubSpot.Client;

public class MyCompanyClass
{
    async Task Test()
    {
        var client = new HubSpotClient(""api"");

        var deal = await client.GetAsync<Deal>(
            0,
            properties: [DealProperties.AmountJson, {|#0:QuoteProperties.ArchivedJson|#0}, ""test""]
        );
    }
}
";

        var expected = new DiagnosticResult("ZRVHS01", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRVHS01MessageFormat)
            .WithArguments("QuoteProperties", "DealProperties");

        await new CSharpAnalyzerTest<ExpectConstInArgumentsAnalyzer, XUnitVerifier>
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
    public async Task ExpectConstInArgumentsAnalyzer_FindsInvalidPropertyConstant_NonGenericMethod()
    {
        const string text = @"
using System.Threading.Tasks;
using ZoneRV.HubSpot.Models.Deal;
using ZoneRV.HubSpot.Models.Quote;
using ZoneRV.HubSpot.Client;
using ZoneRV.HubSpot.Extensions;

public class MyCompanyClass
{
    async Task Test()
    {
        var client = new HubSpotClient(""api"");

        var deal = await client.GetDealForSalesOrderAsync(""salesOrderName"", dealProperties: [DealProperties.AmountJson, {|#0:QuoteProperties.ArchivedJson|#0}]);
    }
}
";

        var expected = new DiagnosticResult("ZRVHS01", DiagnosticSeverity.Error)
            .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
            .WithMessageFormat(Resources.ZRVHS01MessageFormat)
            .WithArguments("QuoteProperties", "DealProperties");

        await new CSharpAnalyzerTest<ExpectConstInArgumentsAnalyzer, XUnitVerifier>
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
    public async Task ExpectConstInArgumentsAnalyzer_FindsValidConstantPropertyForLiteral_GenericMethod()
    {
        const string text = @"
using System.Threading.Tasks;
using ZoneRV.HubSpot.Models.Deal;
using ZoneRV.HubSpot.Models.Quote;
using ZoneRV.HubSpot.Client;

public class MyCompanyClass
{
    async Task Test()
    {
        var client = new HubSpotClient(""api"");

        var deal = await client.GetAsync<Deal>(
            0,
            properties: [""test1"", {|#0:""amount""|#0}, ""test""]
        );
    }
}
";

        var expected = new DiagnosticResult("ZRVHS02", DiagnosticSeverity.Warning)
                      .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRVHS02MessageFormat)
                      .WithArguments("DealProperties", "AmountJson", "amount");

        await new CSharpAnalyzerTest<ExpectConstInArgumentsAnalyzer, XUnitVerifier>
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
    public async Task ExpectConstInArgumentsAnalyzer_FindsValidConstantPropertyForLiteral_NonGenericMethod()
    {
        const string text = @"
using System.Threading.Tasks;
using ZoneRV.HubSpot.Models.Deal;
using ZoneRV.HubSpot.Models.Quote;
using ZoneRV.HubSpot.Client;
using ZoneRV.HubSpot.Extensions;

public class MyCompanyClass
{
    async Task Test()
    {
        var client = new HubSpotClient(""api"");

        var deal = await client.GetDealForSalesOrderAsync(""salesOrderName"", dealProperties: [""test"", {|#0:""amount""|#0}]);
    }
}
";

        var expected = new DiagnosticResult("ZRVHS02", DiagnosticSeverity.Warning)
                      .WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                      .WithMessageFormat(Resources.ZRVHS02MessageFormat)
                      .WithArguments("DealProperties", "AmountJson", "amount");

        await new CSharpAnalyzerTest<ExpectConstInArgumentsAnalyzer, XUnitVerifier>
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