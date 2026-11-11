using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Requests.Tds;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Drives the REAL TDS path against a restored copy of a real database: the rule resolver's six
// scopes and their precedence, a genuine deduction through the settlement choke point (including
// the wallet debit that withholds it), the monthly report, and both of the things that can be done
// with a month — deposit it, or hand it back to the wallet.
//
// It WRITES, and it turns the master switch ON, so it refuses anything that is not obviously a
// throwaway:
//
//   dotnet run --project tools/verify-tds -- FinoTest_ChargeGst

string db = args.FirstOrDefault() ?? string.Empty;
if (!db.StartsWith("FinoTest", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Refusing to run: pass a throwaway database whose name starts with 'FinoTest'.");
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
services.AddScoped<ISettingMasterRepository, SettingMasterRepository>();
services.AddScoped<IWalletMasterRepository, WalletMasterRepository>();
services.AddScoped<IUserWalletMasterRepository, UserWalletMasterRepository>();
services.AddScoped<ITransactionDetailsRepository, TransactionDetailsRepository>();
services.AddScoped<IUserBalanceCappingRepository, UserBalanceCappingRepository>();

services.AddScoped<ITaxEngineService, TaxEngineService>();
services.AddScoped<IPeriodLockService, PeriodLockService>();
services.AddScoped<ILedgerPostingService, LedgerPostingService>();
services.AddScoped<ICommissionTaxResolver, CommissionTaxResolver>();
services.AddScoped<IChargeGstResolver, ChargeGstResolver>();
services.AddScoped<ITdsRuleResolver, TdsRuleResolver>();
services.AddScoped<ITdsDeductionService, TdsDeductionService>();
services.AddScoped<ITdsAdminService, TdsAdminService>();
services.AddScoped<ISlabMasterRepository, SlabMasterRepository>();
services.AddScoped<ISlabCommissionResolver, SlabCommissionResolver>();
services.AddScoped<IProviderConfigurationAdminCommissionService, ProviderConfigurationAdminCommissionService>();
services.AddScoped<IAccountProcessingService, AccountProcessingService>();
services.AddScoped<ISettlementAccountingService, SettlementAccountingService>();

var provider = services.BuildServiceProvider();

int pass = 0, fail = 0;
void Check(string what, decimal actual, decimal expected)
{
    bool ok = actual == expected;
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-58} {actual,12:0.0000}   expected {expected,10:0.0000}");
}
void CheckText(string what, string actual, string expected)
{
    bool ok = string.Equals(actual ?? string.Empty, expected, StringComparison.OrdinalIgnoreCase);
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-58} {actual,12}   expected {expected,10}");
}
void CheckTrue(string what, bool ok)
{
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}");
}
void Section(string t)
{
    Console.WriteLine();
    Console.WriteLine($"-- {t} " + new string('-', Math.Max(0, 76 - t.Length)));
}

Console.WriteLine($"verify-tds - {db}");
Console.WriteLine(new string('=', 92));

// ── A transaction with a real positive commission to deduct from ─────────────────────────────
int txnId, userId, serviceId, userTypeId, walletId;
decimal commission;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var t = await ctx.TransactionMasters.AsNoTracking()
        .Where(x => x.Status == StatusType.Success && x.RetailerCommission > 0m)
        .OrderByDescending(x => x.TransactionMasterID)
        .FirstOrDefaultAsync();
    if (t == null) { Console.WriteLine("No settled transaction with a positive retailer commission in this copy."); return 2; }

    txnId = t.TransactionMasterID; userId = t.UserID; serviceId = t.ServiceID;
    commission = t.RetailerCommission; walletId = t.WalletID;
    userTypeId = await ctx.UserMasters.AsNoTracking()
        .Where(u => u.UserMasterID == userId).Select(u => u.UserTypeID ?? 0).FirstOrDefaultAsync();

    Console.WriteLine($"  txn {txnId}: service {serviceId}, user {userId} (role {userTypeId}), wallet {walletId}, retailer commission {commission:0.00}");
}

// ── The master switch ───────────────────────────────────────────────────────────────────────
Section("The master switch gates everything");
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var row = await ctx.SettingMasters.FirstOrDefaultAsync(s => s.Key == SettingKeys.TDSEnabled && !s.IsDeleted);
    CheckTrue("the switch is seeded by 70_", row != null);
    if (row != null) { row.Value = "false"; row.IsOn = false; await ctx.SaveChangesAsync(); }
}
using (var scope = provider.CreateScope())
{
    bool on = await scope.ServiceProvider.GetRequiredService<ITdsRuleResolver>().IsMasterSwitchOn();
    CheckTrue("reads OFF when the row says false", !on);
}

// Clear any earlier deduction so this run exercises the write.
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var old = await ctx.TransactionTDSDetails.Where(d => d.TransactionMasterID == txnId).ToListAsync();
    ctx.TransactionTDSDetails.RemoveRange(old);
    var tracked = await ctx.TransactionMasters.FirstAsync(t => t.TransactionMasterID == txnId);
    tracked.RetailerTDS = 0m;
    await ctx.SaveChangesAsync();
}

using (var scope = provider.CreateScope())
{
    var result = await scope.ServiceProvider.GetRequiredService<ITdsDeductionService>().DeductForTransaction(txnId);
    CheckTrue("nothing is deducted while the switch is off", result.TotalDeducted == 0m);
    Console.WriteLine($"        said: {result.Note}");
}

// Turn it on for the rest of the run — THROUGH THE REPOSITORY, which is what the Key Setting
// screen does. The settings list is cached in-process for 45 minutes, so flipping the row in raw
// SQL leaves a running API reading the old value; the repository's Update evicts the cache. (The
// first version of this test flipped it with EF directly and spent a run wondering why nothing
// was deducted.)
using (var scope = provider.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<ISettingMasterRepository>();
    var row = (await repo.GetAll()).First(s => s.Key == SettingKeys.TDSEnabled && !s.IsDeleted);
    row.Value = "true"; row.IsOn = true;
    await repo.Update(row);
}
using (var scope = provider.CreateScope())
{
    bool on = await scope.ServiceProvider.GetRequiredService<ITdsRuleResolver>().IsMasterSwitchOn();
    CheckTrue("reads ON once the switch is saved the way the screen saves it", on);
}

// ── Scope precedence ────────────────────────────────────────────────────────────────────────
Section("Six scopes, most specific first");
int globalRule, roleRule, userServiceRule;
using (var scope = provider.CreateScope())
{
    var admin = scope.ServiceProvider.GetRequiredService<ITdsAdminService>();

    var (gid, gf) = await admin.SaveRule(new SaveTdsRuleRequest
    {
        Scope = TdsScope.Global, ChargeType = TdsChargeType.Percent, ChargeValue = 0.01m,
        ApplyOn = TdsApplyOn.CommissionNet, IsActive = true, Remark = "house default",
    });
    CheckTrue($"GLOBAL 0.01% saved{(gf.Length > 0 ? ": " + gf : string.Empty)}", gid > 0);
    globalRule = gid;

    var (rid, rf) = await admin.SaveRule(new SaveTdsRuleRequest
    {
        Scope = TdsScope.UserType, UserTypeID = userTypeId, ChargeType = TdsChargeType.Percent,
        ChargeValue = 2m, ApplyOn = TdsApplyOn.CommissionNet, IsActive = true, Remark = "the role",
    });
    CheckTrue($"USERTYPE 2% saved{(rf.Length > 0 ? ": " + rf : string.Empty)}", rid > 0);
    roleRule = rid;

    var (uid, uf) = await admin.SaveRule(new SaveTdsRuleRequest
    {
        Scope = TdsScope.UserService, UserID = userId, ServiceID = serviceId,
        ChargeType = TdsChargeType.Fix, ChargeValue = 0.5m, ApplyOn = TdsApplyOn.CommissionNet,
        IsActive = true, Remark = "this user on this service",
    });
    CheckTrue($"USER_SERVICE flat Rs 0.50 saved{(uf.Length > 0 ? ": " + uf : string.Empty)}", uid > 0);
    userServiceRule = uid;

    // Validation, which is as much of the feature as the arithmetic.
    var (_, badScope) = await admin.SaveRule(new SaveTdsRuleRequest { Scope = TdsScope.User, ChargeValue = 1m });
    CheckTrue("a USER rule with no user is refused", badScope.Length > 0);
    var (_, badPct) = await admin.SaveRule(new SaveTdsRuleRequest { Scope = TdsScope.Global, ChargeType = TdsChargeType.Percent, ChargeValue = 150m });
    CheckTrue("a percentage over 100 is refused", badPct.Length > 0);
    var (_, dupe) = await admin.SaveRule(new SaveTdsRuleRequest
    {
        Scope = TdsScope.Global, ChargeType = TdsChargeType.Percent, ChargeValue = 0.02m,
        EffectiveFrom = new DateTime(2000, 1, 1),
    });
    CheckTrue("a duplicate of the same scope and date is refused", dupe.Length > 0);
}

using (var scope = provider.CreateScope())
{
    var rules = scope.ServiceProvider.GetRequiredService<ITdsRuleResolver>();

    var d = await rules.Resolve(userId, userTypeId, serviceId, 0, DateTime.Today);
    Check("the USER_SERVICE rule wins", d.TdsRuleID, userServiceRule);
    CheckText("...and it is the flat one", d.ChargeType, TdsChargeType.Fix);
    Console.WriteLine($"        decided by: {d.DecidedBy}");

    // Another service for the same user: the role rule should take over.
    var d2 = await rules.Resolve(userId, userTypeId, serviceId == 10 ? 4 : 10, 0, DateTime.Today);
    Check("on another service the USERTYPE rule wins", d2.TdsRuleID, roleRule);

    // Somebody of another role falls through to the house default.
    var d3 = await rules.Resolve(0, 999, serviceId, 0, DateTime.Today);
    Check("an unmatched role falls through to GLOBAL", d3.TdsRuleID, globalRule);
}

// ── The arithmetic ──────────────────────────────────────────────────────────────────────────
Section("Fix and percent, on the commission net of GST");
using (var scope = provider.CreateScope())
{
    var rules = scope.ServiceProvider.GetRequiredService<ITdsRuleResolver>();

    var pct = new TdsDecision
    {
        RuleFound = true, ChargeType = TdsChargeType.Percent, ChargeValue = 0.01m,
        ApplyOn = TdsApplyOn.CommissionNet, ApplyNoPANRate = false, SectionCode = "194H",
    };
    var c1 = await rules.Compute(pct, userId, 100m, 100m);
    Check("0.01% of 100 (the brief's own example)", c1.TDSAmount, 0.01m);

    var c2 = await rules.Compute(pct, userId, 15m, 12.71m);
    Check("0.01% of a 12.71 base", c2.TDSAmount, 0.00m);   // 0.0013 rounds to nothing

    var two = new TdsDecision
    {
        RuleFound = true, ChargeType = TdsChargeType.Percent, ChargeValue = 2m,
        ApplyOn = TdsApplyOn.CommissionNet, ApplyNoPANRate = false, SectionCode = "194H",
    };
    var c3 = await rules.Compute(two, userId, 15m, 12.71m);
    Check("2% of 12.71", c3.TDSAmount, 0.25m);

    var fix = new TdsDecision
    {
        RuleFound = true, ChargeType = TdsChargeType.Fix, ChargeValue = 2m,
        ApplyOn = TdsApplyOn.CommissionNet, SectionCode = "194H",
    };
    var c4 = await rules.Compute(fix, userId, 15m, 12.71m);
    Check("a flat Rs 2", c4.TDSAmount, 2.00m);

    var c5 = await rules.Compute(fix, userId, 1.50m, 1.27m);
    Check("a flat Rs 2 is capped by the commission itself", c5.TDSAmount, 1.27m);

    var floored = new TdsDecision
    {
        RuleFound = true, ChargeType = TdsChargeType.Percent, ChargeValue = 2m,
        ApplyOn = TdsApplyOn.CommissionNet, MinCommission = 20m, SectionCode = "194H",
    };
    var c6 = await rules.Compute(floored, userId, 15m, 12.71m);
    Check("below the rule's minimum, nothing is deducted", c6.TDSAmount, 0.00m);
    CheckTrue("...and it says why", c6.SkipReason.Contains("minimum"));

    var capped = new TdsDecision
    {
        RuleFound = true, ChargeType = TdsChargeType.Percent, ChargeValue = 10m,
        ApplyOn = TdsApplyOn.CommissionNet, MaxTDSPerTransaction = 1m, SectionCode = "194H",
    };
    var c7 = await rules.Compute(capped, userId, 100m, 100m);
    Check("the per-transaction cap holds", c7.TDSAmount, 1.00m);

    var c8 = await rules.Compute(two, userId, -5m, -5m);
    Check("a negative commission is a charge PAID, so no TDS", c8.TDSAmount, 0.00m);
}

// ── A real deduction, withheld from the wallet ──────────────────────────────────────────────
Section("A real settlement withholds the money");
decimal balanceBefore, expectedTds;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    balanceBefore = await ctx.UserWalletMasters.AsNoTracking()
        .Where(w => w.UserID == userId && w.WalletID == walletId)
        .Select(w => w.Balance).FirstOrDefaultAsync();
    Console.WriteLine($"        wallet {walletId} balance before: {balanceBefore:0.00}");
}

using (var scope = provider.CreateScope())
{
    var result = await scope.ServiceProvider.GetRequiredService<ITdsDeductionService>().DeductForTransaction(txnId);
    expectedTds = result.TotalDeducted;
    CheckTrue($"a deduction was taken ({result.TotalDeducted:0.00})", result.TotalDeducted > 0m);
    foreach (var r in result.Rows)
        Console.WriteLine($"        {r.Tier}: base {r.CommissionBase:0.00}, {r.ChargeType} -> {r.TDSAmount:0.00}, withheld={r.Withheld}  [{r.Note}]");
}

using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var detail = await ctx.TransactionTDSDetails.AsNoTracking()
        .FirstOrDefaultAsync(d => d.TransactionMasterID == txnId && !d.IsDeleted);
    CheckTrue("the deduction row names the rule that decided it", detail != null && detail.TdsRuleID == userServiceRule);
    CheckTrue("...and records that the money was collected", detail != null && detail.Withheld);
    CheckText("...and the charge type", detail?.ChargeType ?? string.Empty, TdsChargeType.Fix);

    decimal after = await ctx.UserWalletMasters.AsNoTracking()
        .Where(w => w.UserID == userId && w.WalletID == walletId)
        .Select(w => w.Balance).FirstOrDefaultAsync();
    Check("the wallet is lighter by exactly the TDS", balanceBefore - after, expectedTds);

    var txn = await ctx.TransactionMasters.AsNoTracking().FirstAsync(t => t.TransactionMasterID == txnId);
    Check("the transaction mirrors the retailer TDS", txn.RetailerTDS, expectedTds);
}

// Settling again must not deduct twice.
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    decimal before = await ctx.UserWalletMasters.AsNoTracking()
        .Where(w => w.UserID == userId && w.WalletID == walletId).Select(w => w.Balance).FirstOrDefaultAsync();

    var again = await scope.ServiceProvider.GetRequiredService<ITdsDeductionService>().DeductForTransaction(txnId);
    decimal after = await ctx.UserWalletMasters.AsNoTracking()
        .Where(w => w.UserID == userId && w.WalletID == walletId).Select(w => w.Balance).FirstOrDefaultAsync();

    Check("re-settling takes nothing more from the wallet", before - after, 0m);
    CheckTrue("...and says it was already deducted", again.Note.Contains("already", StringComparison.OrdinalIgnoreCase));
}

// ── The month, and the two things that can be done with it ──────────────────────────────────
Section("The monthly list");
string period;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    period = (await ctx.TransactionTDSDetails.AsNoTracking()
        .Where(d => d.TransactionMasterID == txnId && !d.IsDeleted)
        .Select(d => d.PeriodKey).FirstOrDefaultAsync()) ?? string.Empty;
    if (period.Length == 0)
    {
        Console.WriteLine("  No deduction was written, so there is no month to report on. Stopping here.");
        Console.WriteLine($"{pass} passed, {fail + 1} failed");
        return 1;
    }

    var report = await scope.ServiceProvider.GetRequiredService<ITdsAdminService>()
        .MonthlyReport(new TdsMonthlyReportRequest { PeriodKey = period });

    var row = report.Rows.FirstOrDefault(r => r.PartyUserID == userId);
    CheckTrue($"the party appears in {report.PeriodLabel}", row != null);
    CheckTrue("...as pending", row?.Pending == true);
    Check("...with the right figure", row?.TDSAmount ?? -1m, expectedTds);
    Console.WriteLine($"        {report.Note}");
}

Section("Deposit it with the government");
using (var scope = provider.CreateScope())
{
    var admin = scope.ServiceProvider.GetRequiredService<ITdsAdminService>();

    string noRef = await admin.MarkDeposited(new TdsDepositRequest { PeriodKey = period, PartyUserID = userId });
    CheckTrue("a deposit with no challan reference is refused", noRef != StatusType.Ok);

    string ok = await admin.MarkDeposited(new TdsDepositRequest
    {
        PeriodKey = period, PartyUserID = userId, Reference = "0510308-27102026-00042",
        ReferenceDate = DateTime.Today, Remark = "verify-tds", SettledByUserID = 1,
    });
    CheckText("recorded as deposited", ok, StatusType.Ok);

    string twice = await admin.MarkDeposited(new TdsDepositRequest
    {
        PeriodKey = period, PartyUserID = userId, Reference = "0510308-27102026-00042", SettledByUserID = 1,
    });
    CheckTrue("the same month cannot be deposited twice", twice != StatusType.Ok);

    var (_, cannotAlsoReturn) = await admin.SettleToWallet(new TdsWalletSettleRequest
    {
        PeriodKey = period, PartyUserID = userId, Remark = "trying both", SettledByUserID = 1,
    });
    CheckTrue("a deposited month cannot also be returned to the wallet", cannotAlsoReturn.Length > 0);
}

Section("Or hand it back to the wallet");
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Withdraw the deposit record so the other branch can be exercised on the same month.
    var s = await ctx.TdsSettlements.FirstAsync(x => x.PeriodKey == period && x.PartyUserID == userId && !x.IsDeleted);
    s.IsDeleted = true;
    await ctx.SaveChangesAsync();
}

using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var admin = scope.ServiceProvider.GetRequiredService<ITdsAdminService>();

    var (_, noReason) = await admin.SettleToWallet(new TdsWalletSettleRequest
    {
        PeriodKey = period, PartyUserID = userId, SettledByUserID = 1,
    });
    CheckTrue("returning money with no reason given is refused", noReason.Length > 0);

    decimal before = await ctx.UserWalletMasters.AsNoTracking()
        .Where(w => w.UserID == userId && w.WalletID == walletId).Select(w => w.Balance).FirstOrDefaultAsync();

    var (credited, failure) = await admin.SettleToWallet(new TdsWalletSettleRequest
    {
        PeriodKey = period, PartyUserID = userId, Remark = "PAN produced; deduction not due", SettledByUserID = 1,
    });
    CheckTrue($"credited back{(failure.Length > 0 ? ": " + failure : string.Empty)}", failure.Length == 0 && credited > 0m);

    decimal after = await ctx.UserWalletMasters.AsNoTracking()
        .Where(w => w.UserID == userId && w.WalletID == walletId).Select(w => w.Balance).FirstOrDefaultAsync();
    Check("the wallet is heavier by exactly what was returned", after - before, credited);
    Check("...which is what was withheld in the first place", credited, expectedTds);

    var (_, twice) = await admin.SettleToWallet(new TdsWalletSettleRequest
    {
        PeriodKey = period, PartyUserID = userId, Remark = "again", SettledByUserID = 1,
    });
    CheckTrue("the same month cannot be returned twice", twice.Length > 0);
}

Console.WriteLine();
Console.WriteLine(new string('=', 92));
Console.WriteLine($"{pass} passed, {fail} failed");
return fail == 0 ? 0 : 1;
