using FintechApp.Data;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Gst;
using FintechApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Prints the GST collection report for a period, straight out of the real service. READ-ONLY -
// it writes nothing and resolves nothing, so it is safe against a live database, and it is the
// quickest way to see the figures the screen will show.
//
//   dotnet run --project tools/report-gst-collection -- finocore_db 2026-10-01 2026-10-31

string db = args.Length > 0 ? args[0] : "finocore_db";
DateTime? from = args.Length > 1 && DateTime.TryParse(args[1], out var f) ? f : null;
DateTime? to = args.Length > 2 && DateTime.TryParse(args[2], out var t) ? t : null;

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(
    $"Server=localhost\\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"));
services.AddScoped<IGstCollectionReportService, GstCollectionReportService>();
var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
var report = await scope.ServiceProvider.GetRequiredService<IGstCollectionReportService>()
    .Build(new GstCollectionReportRequest { FromDate = from, ToDate = to });

Console.WriteLine($"GST collection - {db} - {report.PeriodLabel}");
Console.WriteLine(new string('=', 104));
Console.WriteLine();
Console.WriteLine($"  Output tax stamped on sales      {report.OutputTaxStamped,14:0.00}");
Console.WriteLine($"  + GST on provider commission     {report.GSTOnProviderCommission,14:0.00}");
Console.WriteLine($"  = TOTAL OUTPUT                   {report.TotalOutput,14:0.00}");
Console.WriteLine();
Console.WriteLine($"  Input credit on provider charges {report.InputCreditOnProviderCharges,14:0.00}");
Console.WriteLine($"  + on downline commission         {report.InputCreditOnDownlineCommission,14:0.00}");
Console.WriteLine($"  = TOTAL INPUT CREDIT             {report.TotalInputCredit,14:0.00}");
Console.WriteLine();
Console.WriteLine($"  NET PAYABLE                      {report.NetPayable,14:0.00}");
Console.WriteLine($"  (GST collected on charges: {report.GSTCollectedOnCharges:0.00})");

Console.WriteLine();
Console.WriteLine("-- Inclusive or exclusive, how much " + new string('-', 68));
Console.WriteLine($"  {"Treatment",-14}{"Kind",-8}{"Source",-38}{"Base",14}{"GST",12}{"Txns",7}");
foreach (var r in report.ByTreatment)
    Console.WriteLine($"  {r.Treatment,-14}{r.Kind,-8}{r.Source,-38}{r.BaseValue,14:0.00}{r.GSTAmount,12:0.00}{r.TransactionCount,7}");

Console.WriteLine();
Console.WriteLine("-- Output tax as the returns see it " + new string('-', 68));
Console.WriteLine($"  {"Basis",-14}{"Supply",-14}{"Taxable",14}{"CGST",11}{"SGST",11}{"IGST",11}{"Total",12}{"Txns",7}");
foreach (var r in report.ByBasis)
    Console.WriteLine($"  {r.BillingBasis,-14}{r.SupplyNature,-14}{r.TaxableValue,14:0.00}{r.CGSTAmount,11:0.00}{r.SGSTAmount,11:0.00}{r.IGSTAmount,11:0.00}{r.TotalTax,12:0.00}{r.TransactionCount,7}");

Console.WriteLine();
Console.WriteLine("-- Per service " + new string('-', 88));
Console.WriteLine($"  {"Service",-26}{"Basis",-12}{"SAC",-10}{"Turnover",14}{"Output",11}{"Collected",11}{"Paid",11}{"Txns",6}");
foreach (var r in report.ByService)
    Console.WriteLine($"  {Cut(r.ServiceName, 25),-26}{r.BillingBasis,-12}{r.SACCode,-10}{r.Turnover,14:0.00}{r.OutputTaxStamped,11:0.00}{r.GSTOnChargesCollected,11:0.00}{r.GSTOnAmountsPaid,11:0.00}{r.TransactionCount,6}");

Console.WriteLine();
Console.WriteLine("-- Coverage and differences " + new string('-', 76));
Console.WriteLine($"  successful {report.SuccessCount}, charge-stamped {report.ChargeStampedCount}, "
                + $"not charge-stamped {report.ChargeUnstampedCount}, tax-stamped {report.TaxStampedCount}, "
                + $"no tax stamp {report.TaxUnstampedCount}");
foreach (var n in report.Notes)
{
    Console.WriteLine();
    Console.WriteLine($"  [{n.Level}] {n.Title}");
    foreach (var line in Wrap(n.Detail, 96)) Console.WriteLine($"           {line}");
}

Console.WriteLine();
Console.WriteLine(new string('=', 104));
Console.WriteLine(report.Summary);
return 0;

static string Cut(string s, int n) => s.Length <= n ? s : s.Substring(0, n - 1) + "~";

static IEnumerable<string> Wrap(string text, int width)
{
    var words = (text ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var line = new System.Text.StringBuilder();
    foreach (var w in words)
    {
        if (line.Length + w.Length + 1 > width) { yield return line.ToString(); line.Clear(); }
        if (line.Length > 0) line.Append(' ');
        line.Append(w);
    }
    if (line.Length > 0) yield return line.ToString();
}
