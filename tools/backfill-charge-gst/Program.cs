using FintechApp.Data;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Stamps the charge/GST breakdown on transactions that settled BEFORE the breakdown existed.
//
// It calls the same writer a live settlement calls — ISettlementAccountingService — so there is
// exactly one copy of the formula in the product and a backfilled transaction is indistinguishable
// from one settled today. It never touches a transaction that already carries a stamp, and never a
// FAILED one: a failed transaction was refunded in full and owes no tax.
//
//   dotnet run --project tools/backfill-charge-gst -- --db finocore_db --service 10
//   dotnet run --project tools/backfill-charge-gst -- --db finocore_db --service 10 --apply
//
// Without --apply it writes nothing and prints what it would do. Run the dry run first and read
// the "decided by" lines: if they say a 0% rate was used, fix the configuration before applying,
// because a 0% split books the tax as income.

string db = Arg("--db") ?? string.Empty;
bool apply = args.Contains("--apply", StringComparer.OrdinalIgnoreCase);
int serviceId = int.TryParse(Arg("--service"), out var s) ? s : 0;
int limit = int.TryParse(Arg("--limit"), out var l) ? l : 0;
DateTime? from = DateTime.TryParse(Arg("--from"), out var f) ? f.Date : null;

if (db.Length == 0)
{
    Console.WriteLine("usage: --db <database> [--service <id>] [--from yyyy-MM-dd] [--limit n] [--apply]");
    return 2;
}

string connection =
    $"Server=localhost\\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

var services = new ServiceCollection();
services.AddMemoryCache();
services.AddSingleton(typeof(FintechApp.Processing.MemoryCacheProcessing<>));
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connection));

services.AddScoped<ITaxConfigurationRepository, TaxConfigurationRepository>();
services.AddScoped<IInvoiceSettingRepository, InvoiceSettingRepository>();
services.AddScoped<ISlabDetailRepository, SlabDetailRepository>();
services.AddScoped<IUserSlabMappingRepository, UserSlabMappingRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IProviderConfigurationAdminCommissionRepository, ProviderConfigurationAdminCommissionRepository>();

services.AddScoped<ITaxEngineService, TaxEngineService>();
services.AddScoped<IPeriodLockService, PeriodLockService>();
services.AddScoped<ILedgerPostingService, LedgerPostingService>();
services.AddScoped<ICommissionTaxResolver, CommissionTaxResolver>();
services.AddScoped<IChargeGstResolver, ChargeGstResolver>();
services.AddScoped<ITdsDeductionService, TdsDeductionService>();
services.AddScoped<ISlabMasterRepository, SlabMasterRepository>();
services.AddScoped<ISlabCommissionResolver, SlabCommissionResolver>();
services.AddScoped<IProviderConfigurationAdminCommissionService, ProviderConfigurationAdminCommissionService>();
services.AddScoped<ISettlementAccountingService, SettlementAccountingService>();

var provider = services.BuildServiceProvider();

Console.WriteLine($"backfill-charge-gst — {db}{(apply ? "   *** APPLYING ***" : "   (dry run, nothing written)")}");
if (serviceId > 0) Console.WriteLine($"  service {serviceId} only");
if (from != null) Console.WriteLine($"  settled on or after {from:yyyy-MM-dd}");
Console.WriteLine(new string('=', 100));

List<int> ids;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var q = ctx.TransactionMasters.AsNoTracking()
        .Where(t => !t.IsDeleted
                 && t.Status == StatusType.Success
                 && t.ChargeGSTStampedOn == null
                 // Nothing to split where no charge was recorded on either side.
                 && (t.ProviderCommission != 0m || t.RetailerCommission != 0m
                     || t.DistributerCommission != 0m || t.SuperDistributerCommission != 0m));

    if (serviceId > 0) q = q.Where(t => t.ServiceID == serviceId);
    if (from != null) q = q.Where(t => t.DateTime >= from);

    q = q.OrderBy(t => t.TransactionMasterID);
    if (limit > 0) q = q.Take(limit);

    ids = await q.Select(t => t.TransactionMasterID).ToListAsync();
}

Console.WriteLine($"  {ids.Count} settled transaction(s) carry a charge and no breakdown.");
if (ids.Count == 0) return 0;

int stamped = 0, skipped = 0, warned = 0;
decimal providerGst = 0m, providerBase = 0m, retailerGst = 0m, retailerBase = 0m;
var decisions = new Dictionary<string, int>(StringComparer.Ordinal);

Console.WriteLine();
Console.WriteLine($"  {"Txn",-8}{"Amount",12}{"Prov chg",10}{"Prov GST",10}{"Prov base",11}{"Retl chg",10}{"Retl GST",10}{"Retl base",11}  GST type");
Console.WriteLine("  " + new string('-', 96));

foreach (var id in ids)
{
    using var scope = provider.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var txn = await ctx.TransactionMasters.AsNoTracking().FirstAsync(t => t.TransactionMasterID == id);

    // Resolve exactly what a settlement would, so the dry run shows the figures that will be
    // written rather than an approximation of them.
    var charges = scope.ServiceProvider.GetRequiredService<IChargeGstResolver>();
    var p = await charges.ForProviderCharge(
        txn.ProviderID, txn.ServiceID, txn.OperatorID, txn.Amount, txn.ProviderCommission, txn.DateTime);
    var side = await charges.ForSlabSide(
        txn.UserID, txn.ServiceID, txn.OperatorID, txn.Amount,
        txn.RetailerCommission, txn.DistributerCommission, txn.SuperDistributerCommission, txn.DateTime);

    Console.WriteLine($"  {id,-8}{txn.Amount,12:0.00}{p.ChargeAsQuoted,10:0.00}{p.GSTAmount,10:0.00}{p.BaseCharge,11:0.00}"
                    + $"{side.Retailer.ChargeAsQuoted,10:0.00}{side.Retailer.GSTAmount,10:0.00}{side.Retailer.BaseCharge,11:0.00}"
                    + $"  {p.GSTType}/{side.Retailer.GSTType}");

    foreach (var d in new[] { p.DecidedBy, side.Retailer.DecidedBy }.Where(x => !string.IsNullOrWhiteSpace(x)))
        decisions[d] = decisions.TryGetValue(d, out var n) ? n + 1 : 1;

    foreach (var w in new[] { p.Warning, side.Retailer.Warning }.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
    {
        Console.WriteLine($"           ! {w}");
        warned++;
    }

    providerGst += p.GSTAmount; providerBase += p.BaseCharge;
    retailerGst += side.Retailer.GSTAmount; retailerBase += side.Retailer.BaseCharge;

    if (!apply) { skipped++; continue; }

    bool wrote = await scope.ServiceProvider.GetRequiredService<ISettlementAccountingService>()
        .StampChargeGstForSettledTransaction(id);
    if (wrote) stamped++; else skipped++;
}

Console.WriteLine();
Console.WriteLine("  " + new string('-', 96));
Console.WriteLine($"  provider: GST {providerGst,12:0.00}   base {providerBase,14:0.00}");
Console.WriteLine($"  retailer: GST {retailerGst,12:0.00}   base {retailerBase,14:0.00}");
Console.WriteLine();
Console.WriteLine("  Which layer decided:");
foreach (var kv in decisions.OrderByDescending(k => k.Value))
    Console.WriteLine($"    {kv.Value,5} x  {kv.Key}");

Console.WriteLine();
if (apply)
{
    Console.WriteLine($"  stamped {stamped}, left alone {skipped}" + (warned > 0 ? $", {warned} warning(s) above" : string.Empty));
    Console.WriteLine("  Reports read the stamp from now on. Re-running this changes nothing: a stamped");
    Console.WriteLine("  transaction keeps the figures it was stamped with.");
}
else
{
    Console.WriteLine($"  Dry run — nothing was written. Re-run with --apply to stamp these {ids.Count}.");
    if (warned > 0) Console.WriteLine($"  {warned} warning(s) above. Read them before applying.");
}

return 0;

string? Arg(string name)
{
    int i = Array.FindIndex(args, a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
