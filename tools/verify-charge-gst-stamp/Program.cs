using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Drives the REAL settlement choke point — ISettlementAccountingService.OnTransactionSettled —
// against a restored copy of a real database, and checks that the charge/GST breakdown it stamps
// on the transaction is right, does not move when the same transaction is settled again, and is
// cleared when the transaction is refunded.
//
// It WRITES. It takes the database name as its only argument and refuses anything that is not
// obviously a throwaway, because the live install shares this server:
//
//   dotnet run --project tools/verify-charge-gst-stamp -- FinoTest_ChargeGst
//
// Make the copy first:  BACKUP DATABASE <live> ... WITH COPY_ONLY, then RESTORE ... WITH MOVE.

string db = args.FirstOrDefault() ?? string.Empty;
if (!db.StartsWith("FinoTest", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Refusing to run: pass a throwaway database whose name starts with 'FinoTest'.");
    Console.WriteLine("  dotnet run --project tools/verify-charge-gst-stamp -- FinoTest_ChargeGst");
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

int pass = 0, fail = 0;
void Check(string what, decimal actual, decimal expected)
{
    bool ok = actual == expected;
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-58} {actual,12:0.00}   expected {expected,10:0.00}");
}
void CheckText(string what, string actual, string expected)
{
    bool ok = string.Equals(actual ?? string.Empty, expected, StringComparison.OrdinalIgnoreCase);
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-58} {actual,12}   expected {expected,10}");
}

Console.WriteLine($"verify-charge-gst-stamp — settling against {db}");
Console.WriteLine(new string('=', 80));

// ── Pick a settled transaction that actually carries both sides of a charge ──────────────────
int txnId;
decimal amount, providerQuoted, retailerQuoted;
int serviceId, operatorId, providerId, userId;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var candidate = await ctx.TransactionMasters.AsNoTracking()
        .Where(t => t.Status == StatusType.Success && t.ProviderCommission != 0m && t.RetailerCommission != 0m)
        .OrderByDescending(t => t.TransactionMasterID)
        .FirstOrDefaultAsync();

    if (candidate == null)
    {
        Console.WriteLine("No settled transaction with both a provider and a retailer charge in this copy.");
        return 2;
    }

    txnId = candidate.TransactionMasterID;
    amount = candidate.Amount;
    providerQuoted = candidate.ProviderCommission;
    retailerQuoted = candidate.RetailerCommission;
    serviceId = candidate.ServiceID;
    operatorId = candidate.OperatorID;
    providerId = candidate.ProviderID;
    userId = candidate.UserID;

    Console.WriteLine($"  transaction {txnId}: service {serviceId}, operator {operatorId}, provider {providerId}, "
                    + $"user {userId}, amount {amount:0.00}");
    Console.WriteLine($"  provider charge {providerQuoted:0.00}, retailer charge {retailerQuoted:0.00}");
}

// ── Set both bands to INCLUSIVE 18%, which is how this card is actually quoted ───────────────
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var band = await ctx.ProviderConfigurationAdminCommissions
        .Where(b => !b.IsDeleted && b.ProviderID == providerId && b.ServiceID == serviceId
                 && amount >= b.FromAmount && (b.ToAmount <= 0 || amount <= b.ToAmount))
        .OrderByDescending(b => b.OperatorID > 0)
        .FirstOrDefaultAsync();

    int slabMasterId = await ctx.UserSlabMappings.AsNoTracking()
        .Where(m => !m.IsDeleted && m.UserID == userId)
        .OrderByDescending(m => m.UserSlabMappingID)
        .Select(m => m.SlabMasterID)
        .FirstOrDefaultAsync();

    var slabBand = await ctx.SlabDetails
        .Where(s => !s.IsDeleted && s.IsActive && s.SlabMasterID == slabMasterId
                 && (s.ServiceID == serviceId || s.ServiceID == 0)
                 && (s.OperatorID == operatorId || s.OperatorID == 0)
                 && amount >= s.FromAmount && amount <= s.ToAmount)
        .OrderBy(s => s.ToAmount - s.FromAmount)
        .FirstOrDefaultAsync();

    Console.WriteLine($"  provider band: {(band == null ? "none" : $"#{band.ProviderConfigurationAdminCommissionID} "
        + $"{band.FromAmount:0}-{band.ToAmount:0} = {band.AdminComm:0.00} {band.AdminCommType}")}");
    Console.WriteLine($"  slab band:     {(slabBand == null ? "none" : $"#{slabBand.SlabDetailID} on slab {slabMasterId} "
        + $"{slabBand.FromAmount:0}-{slabBand.ToAmount:0} = {slabBand.RetailerComm:0.00} {slabBand.RetailerCommType}")}");

    if (band != null) { band.GSTType = ChargeGstType.Inclusive; band.GSTRatePercent = 18m; }
    if (slabBand != null) { slabBand.GSTType = ChargeGstType.Inclusive; slabBand.GSTRatePercent = 18m; }

    // Start from unstamped, so this run exercises the write rather than reading an earlier one.
    var txn = await ctx.TransactionMasters.FirstAsync(t => t.TransactionMasterID == txnId);
    txn.ChargeGSTStampedOn = null;
    await ctx.SaveChangesAsync();
}

decimal Inclusive18(decimal charge)
{
    decimal m = Math.Abs(charge);
    decimal gst = Math.Round(m * 18m / 118m, 2, MidpointRounding.AwayFromZero);
    return charge < 0 ? -gst : gst;
}

// ── Settle it, for real, through the one choke point ────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("── Settling (OnTransactionSettled) " + new string('─', 45));
using (var scope = provider.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ISettlementAccountingService>().OnTransactionSettled(txnId);
}

using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var t = await ctx.TransactionMasters.AsNoTracking().FirstAsync(x => x.TransactionMasterID == txnId);

    bool stamped = t.ChargeGSTStampedOn != null;
    if (stamped) pass++; else fail++;
    Console.WriteLine($"  {(stamped ? "PASS" : "FAIL")}  the breakdown was stamped"
                    + (stamped ? $" at {t.ChargeGSTStampedOn:yyyy-MM-dd HH:mm:ss}" : " — nothing was written"));

    CheckText("provider GST type", t.ProviderGSTType, ChargeGstType.Inclusive);
    Check("provider GST percent", t.ProviderGSTPercent, 18m);
    Check("provider GST amount", t.ProviderGSTAmount, Inclusive18(providerQuoted));
    Check("provider base charge", t.ProviderBaseCharge, providerQuoted - Inclusive18(providerQuoted));
    Check("provider total charge (inclusive: unchanged)", t.ProviderTotalCharge, Math.Round(providerQuoted, 2));

    CheckText("retailer GST type", t.RetailerGSTType, ChargeGstType.Inclusive);
    Check("retailer GST amount", t.RetailerGST, Inclusive18(retailerQuoted));
    Check("retailer base charge", t.RetailerBaseCharge, retailerQuoted - Inclusive18(retailerQuoted));

    Console.WriteLine($"        provider: {t.ProviderGSTSource}");
    Console.WriteLine($"        retailer: {t.RetailerGSTSource}");
}

// ── Settle it again: the stamp must not move ────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("── Re-settling (a status sweep, a recon run, a manual settle) " + new string('─', 19));
DateTime? firstStamp;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    firstStamp = (await ctx.TransactionMasters.AsNoTracking().FirstAsync(x => x.TransactionMasterID == txnId)).ChargeGSTStampedOn;
}
await Task.Delay(1100);
using (var scope = provider.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ISettlementAccountingService>().OnTransactionSettled(txnId);
}
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var t = await ctx.TransactionMasters.AsNoTracking().FirstAsync(x => x.TransactionMasterID == txnId);
    bool same = t.ChargeGSTStampedOn == firstStamp;
    if (same) pass++; else fail++;
    Console.WriteLine($"  {(same ? "PASS" : "FAIL")}  the stamp did not move"
                    + (same ? string.Empty : $" — was {firstStamp:HH:mm:ss}, now {t.ChargeGSTStampedOn:HH:mm:ss}"));
    Check("provider GST amount unchanged", t.ProviderGSTAmount, Inclusive18(providerQuoted));
}

// ── Refund it: a failed transaction earns nothing and owes nothing ──────────────────────────
Console.WriteLine();
Console.WriteLine("── Refunding (OnTransactionRefunded) " + new string('─', 43));
using (var scope = provider.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ISettlementAccountingService>().OnTransactionRefunded(txnId);
}
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var t = await ctx.TransactionMasters.AsNoTracking().FirstAsync(x => x.TransactionMasterID == txnId);

    bool cleared = t.ChargeGSTStampedOn == null;
    if (cleared) pass++; else fail++;
    Console.WriteLine($"  {(cleared ? "PASS" : "FAIL")}  the stamp was cleared");
    Check("provider GST amount", t.ProviderGSTAmount, 0m);
    Check("provider base charge", t.ProviderBaseCharge, 0m);
    Check("retailer GST amount", t.RetailerGST, 0m);
    Check("retailer base charge", t.RetailerBaseCharge, 0m);
    CheckText("provider GST type", t.ProviderGSTType ?? string.Empty, string.Empty);
}

// ── And the preview, over the same real configuration ───────────────────────────────────────
Console.WriteLine();
Console.WriteLine("── Preview of the same transaction size " + new string('─', 40));
using (var scope = provider.CreateScope())
{
    var preview = await scope.ServiceProvider.GetRequiredService<IChargeGstResolver>()
        .Preview(userId, providerId, serviceId, operatorId, amount, DateTime.Today);

    Console.WriteLine($"  provider  charge {preview.Provider.ChargeAsQuoted,10:0.00}  GST {preview.Provider.GSTAmount,8:0.00}  base {preview.Provider.BaseCharge,10:0.00}");
    Console.WriteLine($"  retailer  charge {preview.Retailer.ChargeAsQuoted,10:0.00}  GST {preview.Retailer.GSTAmount,8:0.00}  base {preview.Retailer.BaseCharge,10:0.00}");
    Console.WriteLine($"  net to the house {preview.NetProfit,10:0.00}");
    foreach (var note in preview.Notes) Console.WriteLine($"    - {note}");
}

Console.WriteLine();
Console.WriteLine(new string('=', 80));
Console.WriteLine($"{pass} passed, {fail} failed");
return fail == 0 ? 0 : 1;
