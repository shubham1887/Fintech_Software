using FintechApp.Data;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Stamps the sale tax on settled transactions that carry NO stamp, through the real settlement
// service - so the figures are resolved exactly as a live settlement resolves them.
//
//   dotnet run --project tools/stamp-tax -- --db finocore_db --service 10 --from 2026-10-01
//   ...add --apply to write.
//
// IT ONLY EVER ADDS A MISSING STAMP. It cannot replace one, and that is not a policy here - it is
// the database: `UX_TransactionTaxDetails_TransactionMasterID` is UNIQUE and unfiltered, so one
// transaction may carry exactly one tax-detail row for all time, soft-deleted ones included. The
// design is deliberate ("the stamp must not move once written"), and it means correcting a stamp
// that is already wrong is a deliberate feature with its own audit trail, not a flag on a tool.
//
// A first version of this tool tried soft-delete-then-insert. On a throwaway copy that left 106
// transactions with no live stamp at all: the insert hit the unique index, the settlement service
// caught the DbUpdateException exactly as it is designed to, and re-read nothing. Hence this note.

string db = Arg("--db") ?? string.Empty;
bool apply = Has("--apply");
const bool restamp = false;
int serviceId = int.TryParse(Arg("--service"), out var s) ? s : 0;
int limit = int.TryParse(Arg("--limit"), out var l) ? l : 0;
DateTime? from = DateTime.TryParse(Arg("--from"), out var f) ? f.Date : null;

if (db.Length == 0) { Console.WriteLine("usage: --db <database> [--service n] [--from yyyy-MM-dd] [--limit n] [--apply]"); return 2; }

var services = new ServiceCollection();
services.AddMemoryCache();
services.AddSingleton(typeof(FintechApp.Processing.MemoryCacheProcessing<>));
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(
    @$"Server=localhost\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"));
services.AddScoped<ITaxConfigurationRepository, TaxConfigurationRepository>();
services.AddScoped<IInvoiceSettingRepository, InvoiceSettingRepository>();
services.AddScoped<ISlabDetailRepository, SlabDetailRepository>();
services.AddScoped<IUserSlabMappingRepository, UserSlabMappingRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IProviderConfigurationAdminCommissionRepository, ProviderConfigurationAdminCommissionRepository>();
services.AddScoped<ISettingMasterRepository, SettingMasterRepository>();
services.AddScoped<IUserWalletMasterRepository, UserWalletMasterRepository>();
services.AddScoped<IUserBalanceCappingRepository, UserBalanceCappingRepository>();
services.AddScoped<ITransactionDetailsRepository, TransactionDetailsRepository>();
services.AddScoped<ITaxEngineService, TaxEngineService>();
services.AddScoped<IPeriodLockService, PeriodLockService>();
services.AddScoped<ILedgerPostingService, LedgerPostingService>();
services.AddScoped<ICommissionTaxResolver, CommissionTaxResolver>();
services.AddScoped<IChargeGstResolver, ChargeGstResolver>();
services.AddScoped<ITdsRuleResolver, TdsRuleResolver>();
services.AddScoped<ITdsDeductionService, TdsDeductionService>();
services.AddScoped<ISlabMasterRepository, SlabMasterRepository>();
services.AddScoped<ISlabCommissionResolver, SlabCommissionResolver>();
services.AddScoped<IProviderConfigurationAdminCommissionService, ProviderConfigurationAdminCommissionService>();
services.AddScoped<IAccountProcessingService, AccountProcessingService>();
services.AddScoped<ISettlementAccountingService, SettlementAccountingService>();
var provider = services.BuildServiceProvider();

Console.WriteLine($"stamp-tax - {db}{(apply ? "   *** APPLYING ***" : "   (dry run)")}{(restamp ? "   [RESTAMP]" : string.Empty)}");
Console.WriteLine(new string('=', 92));

List<int> ids;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var stampedIds = ctx.TransactionTaxDetails.Where(d => !d.IsDeleted).Select(d => d.TransactionMasterID);

    var q = ctx.TransactionMasters.AsNoTracking()
        .Where(t => !t.IsDeleted && t.Status == StatusType.Success);
    if (serviceId > 0) q = q.Where(t => t.ServiceID == serviceId);
    if (from != null) q = q.Where(t => t.DateTime >= from);
    q = restamp ? q : q.Where(t => !stampedIds.Contains(t.TransactionMasterID));
    q = q.OrderBy(t => t.TransactionMasterID);
    if (limit > 0) q = q.Take(limit);
    ids = await q.Select(t => t.TransactionMasterID).ToListAsync();
}

Console.WriteLine($"  {ids.Count} transaction(s) match.");
if (ids.Count == 0) return 0;

Console.WriteLine();
Console.WriteLine($"  {"Txn",-8}{"Basis",-13}{"Supply",-13}{"Taxable",12}{"Tax",10}   charge-stamp GST");
Console.WriteLine("  " + new string('-', 80));

int done = 0;
decimal taxTotal = 0m, chargeTotal = 0m;
foreach (var id in ids)
{
    using var scope = provider.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (apply)
    {
        if (restamp)
        {
            // Withdraw the old stamp rather than deleting it: the row stays, marked deleted, so
            // what the transaction USED to say about its tax is still answerable.
            var old = await ctx.TransactionTaxDetails
                .Where(d => d.TransactionMasterID == id && !d.IsDeleted).ToListAsync();
            foreach (var o in old) { o.IsDeleted = true; o.UpdatedateTime = DateTime.Now; }
            if (old.Count > 0) await ctx.SaveChangesAsync();
        }
        await scope.ServiceProvider.GetRequiredService<ISettlementAccountingService>().OnTransactionSettled(id);
        done++;
    }

    var stamp = await ctx.TransactionTaxDetails.AsNoTracking()
        .FirstOrDefaultAsync(d => d.TransactionMasterID == id && !d.IsDeleted);
    var txn = await ctx.TransactionMasters.AsNoTracking().FirstAsync(t => t.TransactionMasterID == id);

    decimal tax = stamp == null ? 0m : stamp.CGSTAmount + stamp.SGSTAmount + stamp.IGSTAmount + stamp.CessAmount;
    decimal chargeGst = Math.Abs(txn.RetailerGST < 0m ? txn.RetailerGST : 0m);
    taxTotal += tax; chargeTotal += chargeGst;

    if (ids.Count <= 30 || done <= 10)
        Console.WriteLine($"  {id,-8}{stamp?.BillingBasis ?? "-",-13}{stamp?.SupplyNature ?? "-",-13}"
                        + $"{stamp?.TaxableValue ?? 0m,12:0.00}{tax,10:0.00}   {chargeGst,10:0.00}"
                        + (Math.Abs(tax - chargeGst) <= 0.02m ? "  match" : string.Empty));
}

Console.WriteLine();
Console.WriteLine($"  output tax stamped {taxTotal,12:0.00}");
Console.WriteLine($"  charge-stamp GST   {chargeTotal,12:0.00}");
Console.WriteLine($"  difference         {taxTotal - chargeTotal,12:0.00}");
Console.WriteLine();
Console.WriteLine(apply ? $"  stamped {done}." : "  Dry run - nothing was written.");
return 0;

string? Arg(string name)
{
    int i = Array.FindIndex(args, a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
bool Has(string name) => args.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
