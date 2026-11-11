using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Requests.CashCollection;
using FintechApp.Requests.Fund;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Drives the REAL outstanding / cash-collection path against a restored copy of a real database:
// credit extended on a fund transfer, the credit limit, the collection, over-collection, who is
// allowed to collect from whom, what a reversal does to the debt, and whether the statement ties
// out to the balance.
//
// It WRITES. It refuses anything that is not obviously a throwaway:
//
//   dotnet run --project tools/verify-outstanding -- FinoTest_Outstanding

string db = args.FirstOrDefault() ?? string.Empty;
if (!db.StartsWith("FinoTest", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Refusing to run: pass a throwaway database whose name starts with 'FinoTest'.");
    return 2;
}

string connection =
    $"Server=localhost\\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

ServiceProvider BuildProvider()
{
var services = new ServiceCollection();
services.AddMemoryCache();
services.AddLogging();
services.AddSingleton(typeof(FintechApp.Processing.MemoryCacheProcessing<>));
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connection));

services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IFundTransactionRepository, FundTransactionRepository>();
services.AddScoped<IUserWalletMasterRepository, UserWalletMasterRepository>();
services.AddScoped<IWalletMasterRepository, WalletMasterRepository>();
services.AddScoped<IOTPMasterRepository, OTPMasterRepository>();
services.AddScoped<IEcollectionDetailsRepository, EcollectionDetailsRepository>();
services.AddScoped<IModeRepository, ModeRepository>();
services.AddScoped<ISubModeRepository, SubModeRepository>();
services.AddScoped<ISettingMasterRepository, SettingMasterRepository>();
services.AddScoped<ICashCollectionMasterRepository, CashCollectionMasterRepository>();
services.AddScoped<ICashCollectionDetailRepository, CashCollectionDetailRepository>();
services.AddScoped<IServiceRepository, ServiceRepository>();
services.AddScoped<ISystemDowntimeService, SystemDowntimeService>();
services.AddScoped<IMessageService, SilentMessages>();
services.AddScoped<IOutstandingService, OutstandingService>();
services.AddScoped<ICashCollectionMasterService, CashCollectionMasterService>();
services.AddScoped<IFundService, FundService>();

return services.BuildServiceProvider();
}

var sp = BuildProvider();

int passed = 0, failed = 0;
void Check(string what, bool ok, string detail = "")
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail.Length > 0 ? $"  — {detail}" : "")}");
    if (ok) passed++; else failed++;
}

using var scope = sp.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var fund = scope.ServiceProvider.GetRequiredService<IFundService>();
var outstanding = scope.ServiceProvider.GetRequiredService<IOutstandingService>();
var collection = scope.ServiceProvider.GetRequiredService<ICashCollectionMasterService>();
var walletRepo = scope.ServiceProvider.GetRequiredService<IUserWalletMasterRepository>();
var ccMasters = scope.ServiceProvider.GetRequiredService<ICashCollectionMasterRepository>();
var ccDetails = scope.ServiceProvider.GetRequiredService<ICashCollectionDetailRepository>();

// ── The cast ─────────────────────────────────────────────────────────────────────────────
// A distributor and one of their own retailers, taken from the restored data rather than
// invented, so the hierarchy, the wallets and the user types are the real shapes.
const int Distributor = 4;   // parent
const int Retailer = 6;      // child of 4
const int OtherRetailer = 8; // also a child of 4 - used for the "not my downline" case
int walletId = (int)WalletCode.Recharge;

Console.WriteLine($"\nVerifying outstanding / cash collection against {db}");
Console.WriteLine($"  distributor {Distributor} -> retailer {Retailer}, wallet {walletId}\n");

// The Credit mode row, matched by name exactly as the fund flows match it.
int creditModeId = await ctx.Modes
    .Where(m => m.ModeName == OutstandingLedger.CreditModeName && !m.IsDelete)
    .Select(m => m.ModeID).FirstOrDefaultAsync();
Check("the Credit payment mode exists", creditModeId > 0, $"ModeID {creditModeId}");

// A mode that HAS sub modes requires one (ResolveModeSubMode), and Credit ships with three. The
// form offers the picker, so the harness picks too - sending none would be testing the validation
// rather than the feature.
int creditSubModeId = await ctx.SubModes
    .Where(s => s.ModeID == creditModeId && s.IsActive && !s.IsDelete)
    .OrderBy(s => s.DisplayOrder).Select(s => s.SubModeID).FirstOrDefaultAsync();
Check("the Credit mode has a sub mode to choose", creditSubModeId > 0, $"SubModeID {creditSubModeId}");

// A non-credit mode, to prove ordinary funding does NOT touch the outstanding.
int bankModeId = await ctx.Modes
    .Where(m => m.ModeName == "Bank" && !m.IsDelete)
    .Select(m => m.ModeID).FirstOrDefaultAsync();
int bankSubModeId = await ctx.SubModes
    .Where(s => s.ModeID == bankModeId && s.IsActive && !s.IsDelete)
    .Select(s => s.SubModeID).FirstOrDefaultAsync();

// ── Arrange: wallets funded, debt cleared, a 2,00,000 credit limit ───────────────────────
async Task EnsureWallet(int userId, decimal balance)
{
    var row = await ctx.UserWalletMasters.FirstOrDefaultAsync(w => w.UserID == userId && w.WalletID == walletId);
    if (row == null)
    {
        ctx.UserWalletMasters.Add(new UserWalletMaster { UserID = userId, WalletID = walletId, Balance = balance });
    }
    else
    {
        row.Balance = balance;
        row.HoldBalance = 0;
    }
    await ctx.SaveChangesAsync();
}

await EnsureWallet(Distributor, 10_000_000m);
await EnsureWallet(Retailer, 0m);
await EnsureWallet(OtherRetailer, 0m);

// Start every run from a clean slate so a re-run means the same thing as a first run.
await ctx.Database.ExecuteSqlRawAsync(
    "DELETE FROM CashCollectionDetails WHERE UserID IN ({0}, {1})", Retailer, OtherRetailer);
await ctx.Database.ExecuteSqlRawAsync(
    "DELETE FROM CashCollectionMasters WHERE UserID IN ({0}, {1})", Retailer, OtherRetailer);

Check("credit limit set to 2,00,000",
    (await outstanding.SetCreditLimit(Retailer, 200_000m)).EqualsIgnoreCase(StatusType.Ok));

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n1. Ordinary (non-credit) funding must NOT create a debt");
// ─────────────────────────────────────────────────────────────────────────────────────────
string fail = await fund.Transfer(new FundTransferRequest
{
    ReceiverID = Retailer, WalletID = walletId, Amount = 10_000m,
    Remark = $"verify-plain-{Guid.NewGuid():N}",
    ModeID = bankModeId, SubModeID = bankSubModeId,
}, Distributor, "127.0.0.1");

Check("a Bank-mode transfer succeeds", fail.EqualsIgnoreCase(StatusType.Ok), fail);
Check("the wallet was credited", (await Bal(Retailer)) == 10_000m, $"{await Bal(Retailer):F2}");
Check("outstanding is still zero", (await outstanding.GetOutstanding(Retailer)) == 0m);

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n2. Credit funding: the wallet AND the debt move together");
// ─────────────────────────────────────────────────────────────────────────────────────────
decimal walletBefore = await Bal(Retailer);
fail = await fund.Transfer(new FundTransferRequest
{
    ReceiverID = Retailer, WalletID = walletId, Amount = 150_000m,
    Remark = $"verify-credit-{Guid.NewGuid():N}",
    ModeID = creditModeId, SubModeID = creditSubModeId,
}, Distributor, "127.0.0.1");

Check("a Credit-mode transfer of 1,50,000 succeeds", fail.EqualsIgnoreCase(StatusType.Ok), fail);
Check("the wallet was credited by 1,50,000", (await Bal(Retailer)) == walletBefore + 150_000m,
    $"{await Bal(Retailer):F2}");
Check("outstanding is 1,50,000", (await outstanding.GetOutstanding(Retailer)) == 150_000m,
    $"{await outstanding.GetOutstanding(Retailer):F2}");

var rows = (await ccDetails.GetStatement(Retailer, null, null, null, 0, 50));
Check("one statement row was written", rows.Count == 1, $"{rows.Count} row(s)");
Check("it is a CREDIT row of the right type",
    rows.Count == 1 && rows[0].Credit == OutstandingLedger.CreditMarker
                    && rows[0].Type == OutstandingLedger.TypeFundTransfer,
    rows.Count == 1 ? $"{rows[0].Credit}/{rows[0].Type}" : "");
Check("opening 0 -> closing 1,50,000",
    rows.Count == 1 && rows[0].OutstandingOpening == 0m && rows[0].OutstandingClosing == 150_000m);
Check("it is linked back to the fund transaction",
    rows.Count == 1 && rows[0].FundTransactionID > 0, $"FT {rows.FirstOrDefault()?.FundTransactionID}");
Check("it records which wallet the funds landed in",
    rows.Count == 1 && rows[0].WalletID == walletId);

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n3. The credit limit is a ceiling, and nothing moves when it refuses");
// ─────────────────────────────────────────────────────────────────────────────────────────
decimal walletAt150 = await Bal(Retailer);
fail = await fund.Transfer(new FundTransferRequest
{
    ReceiverID = Retailer, WalletID = walletId, Amount = 100_000m, // 1.5L + 1L > 2L limit
    Remark = $"verify-overlimit-{Guid.NewGuid():N}",
    ModeID = creditModeId, SubModeID = creditSubModeId,
}, Distributor, "127.0.0.1");

Check("a credit that would breach the limit is refused", !fail.EqualsIgnoreCase(StatusType.Ok), fail);
Check("the refusal names the limit and what is already owed",
    fail.Contains("limit", StringComparison.OrdinalIgnoreCase) && fail.Contains("200000"), fail);
Check("the wallet was NOT credited", (await Bal(Retailer)) == walletAt150, $"{await Bal(Retailer):F2}");
Check("outstanding is unchanged at 1,50,000", (await outstanding.GetOutstanding(Retailer)) == 150_000m);
Check("no statement row was written for the refusal",
    (await ccDetails.GetStatement(Retailer, null, null, null, 0, 50)).Count == 1);

// Right up to the limit is allowed - the guard is "past the limit", not "near it".
fail = await fund.Transfer(new FundTransferRequest
{
    ReceiverID = Retailer, WalletID = walletId, Amount = 50_000m,
    Remark = $"verify-tolimit-{Guid.NewGuid():N}",
    ModeID = creditModeId, SubModeID = creditSubModeId,
}, Distributor, "127.0.0.1");
Check("a credit that lands exactly ON the limit is allowed", fail.EqualsIgnoreCase(StatusType.Ok), fail);
Check("outstanding is 2,00,000", (await outstanding.GetOutstanding(Retailer)) == 200_000m,
    $"{await outstanding.GetOutstanding(Retailer):F2}");

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n4. Partial collection — the question that started this");
//    3,00,000 of credit taken, 2,50,000 collected, 50,000 still owed. There is no separate
//    "reconcile" step: the statement IS the reconciliation.
// ─────────────────────────────────────────────────────────────────────────────────────────
await outstanding.SetCreditLimit(Retailer, 400_000m);
fail = await fund.Transfer(new FundTransferRequest
{
    ReceiverID = Retailer, WalletID = walletId, Amount = 100_000m,
    Remark = $"verify-to300k-{Guid.NewGuid():N}",
    ModeID = creditModeId, SubModeID = creditSubModeId,
}, Distributor, "127.0.0.1");
Check("topped up to 3,00,000 outstanding",
    fail.EqualsIgnoreCase(StatusType.Ok) && (await outstanding.GetOutstanding(Retailer)) == 300_000m,
    $"{await outstanding.GetOutstanding(Retailer):F2}");

// The collector's MPIN, read from the row rather than guessed.
int? mpin = await ctx.UserMasters.Where(u => u.UserMasterID == Distributor).Select(u => u.Mpin).FirstOrDefaultAsync();
string pin = (mpin ?? 0).ToString();

var (collectFail, collectMsg) = await collection.AddCashCollection(new AddCashCollectionRequest
{
    ReceiverID = Retailer, Amount = 250_000m, PayMode = "Cash",
    UTRNumber = "", Remark = "Part payment received", Pin = pin,
}, Distributor);

Check("the distributor can record a 2,50,000 collection",
    collectFail.EqualsIgnoreCase(StatusType.Ok), collectFail);
Check("50,000 is still outstanding", (await outstanding.GetOutstanding(Retailer)) == 50_000m,
    $"{await outstanding.GetOutstanding(Retailer):F2}");
Check("the collection did NOT touch the wallet", (await Bal(Retailer)) == 310_000m, $"{await Bal(Retailer):F2}");

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n5. Over-collection is refused (a debt cannot go negative)");
// ─────────────────────────────────────────────────────────────────────────────────────────
(collectFail, _) = await collection.AddCashCollection(new AddCashCollectionRequest
{
    ReceiverID = Retailer, Amount = 60_000m, PayMode = "Cash", Remark = "too much", Pin = pin,
}, Distributor);
Check("collecting more than is owed is refused", !collectFail.EqualsIgnoreCase(StatusType.Ok), collectFail);
Check("outstanding is unchanged at 50,000", (await outstanding.GetOutstanding(Retailer)) == 50_000m);

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n6. Who may collect from whom");
// ─────────────────────────────────────────────────────────────────────────────────────────
(collectFail, _) = await collection.AddCashCollection(new AddCashCollectionRequest
{
    ReceiverID = Retailer, Amount = 1_000m, PayMode = "Cash", Remark = "x", Pin = pin,
}, Retailer);
Check("a user cannot clear their own outstanding", !collectFail.EqualsIgnoreCase(StatusType.Ok), collectFail);

(collectFail, _) = await collection.AddCashCollection(new AddCashCollectionRequest
{
    ReceiverID = Retailer, Amount = 1_000m, PayMode = "Cash", Remark = "x", Pin = pin,
}, OtherRetailer);
Check("a sibling cannot collect from someone who is not their downline",
    !collectFail.EqualsIgnoreCase(StatusType.Ok), collectFail);

(collectFail, _) = await collection.AddCashCollection(new AddCashCollectionRequest
{
    ReceiverID = Retailer, Amount = 1_000m, PayMode = "Cash", Remark = "wrong pin", Pin = "000000",
}, Distributor);
Check("a wrong MPIN is refused", !collectFail.EqualsIgnoreCase(StatusType.Ok), collectFail);
Check("outstanding survived all three refusals at 50,000",
    (await outstanding.GetOutstanding(Retailer)) == 50_000m,
    $"{await outstanding.GetOutstanding(Retailer):F2}");

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n7. A reversal takes the debt with the money, capped at what is owed");
// ─────────────────────────────────────────────────────────────────────────────────────────
decimal owedBefore = await outstanding.GetOutstanding(Retailer); // 50,000
fail = await fund.Reverse(new FundReverseWithOTPRequest
{
    SenderID = Retailer, WalletID = walletId, Amount = 20_000m, Remark = "verify partial reverse",
}, Distributor, "127.0.0.1");
Check("a 20,000 reversal succeeds", fail.EqualsIgnoreCase(StatusType.Ok), fail);
Check("outstanding fell by 20,000 to 30,000", (await outstanding.GetOutstanding(Retailer)) == owedBefore - 20_000m,
    $"{await outstanding.GetOutstanding(Retailer):F2}");

// More than is owed: the debt clears to zero and stops there, it does not go negative.
fail = await fund.Reverse(new FundReverseWithOTPRequest
{
    SenderID = Retailer, WalletID = walletId, Amount = 100_000m, Remark = "verify over reverse",
}, Distributor, "127.0.0.1");
Check("a reversal larger than the debt succeeds", fail.EqualsIgnoreCase(StatusType.Ok), fail);
Check("outstanding is exactly zero, not negative", (await outstanding.GetOutstanding(Retailer)) == 0m,
    $"{await outstanding.GetOutstanding(Retailer):F2}");

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n8. The statement ties out to the balance");
// ─────────────────────────────────────────────────────────────────────────────────────────
var all = (await ccDetails.GetStatement(Retailer, null, null, null, 0, 500))
    .OrderBy(r => r.CashCollectionDetailID).ToList();

decimal credits = all.Where(r => r.Credit == OutstandingLedger.CreditMarker).Sum(r => r.OutstandingAmount);
decimal debits = all.Where(r => r.Debit == OutstandingLedger.DebitMarker).Sum(r => r.OutstandingAmount);
decimal balance = await outstanding.GetOutstanding(Retailer);

Check("credits minus debits equals the balance", credits - debits == balance,
    $"{credits:F2} - {debits:F2} = {credits - debits:F2}, balance {balance:F2}");

bool chained = true;
string breakAt = "";
for (int i = 0; i < all.Count; i++)
{
    decimal expectedOpening = i == 0 ? 0m : all[i - 1].OutstandingClosing;
    if (all[i].OutstandingOpening != expectedOpening)
    {
        chained = false;
        breakAt = $"row {all[i].CashCollectionDetailID}: opening {all[i].OutstandingOpening:F2}, expected {expectedOpening:F2}";
        break;
    }
    decimal signed = all[i].Credit == OutstandingLedger.CreditMarker ? all[i].OutstandingAmount : -all[i].OutstandingAmount;
    if (all[i].OutstandingOpening + signed != all[i].OutstandingClosing)
    {
        chained = false;
        breakAt = $"row {all[i].CashCollectionDetailID}: {all[i].OutstandingOpening:F2} {signed:+0.00;-0.00} != {all[i].OutstandingClosing:F2}";
        break;
    }
}
Check("every row's closing is the next row's opening", chained, breakAt);
Check("every row names a counterparty", all.All(r => r.ParentID > 0));
Check("every row carries a remark", all.All(r => !string.IsNullOrWhiteSpace(r.ParentRemark)));

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n9. The feature switch stops NEW credit without hiding old debt");
// ─────────────────────────────────────────────────────────────────────────────────────────
await outstanding.SetCreditLimit(Retailer, 500_000m);
fail = await fund.Transfer(new FundTransferRequest
{
    ReceiverID = Retailer, WalletID = walletId, Amount = 25_000m,
    Remark = $"verify-switch-{Guid.NewGuid():N}", ModeID = creditModeId, SubModeID = creditSubModeId,
}, Distributor, "127.0.0.1");
Check("credit works while the switch is on",
    fail.EqualsIgnoreCase(StatusType.Ok) && (await outstanding.GetOutstanding(Retailer)) == 25_000m, fail);

// Flipped through the REPOSITORY, which is the path the Key Settings screen takes - and the only
// one that works. Key Settings are held in a process-wide cache for 45 minutes, so writing the
// row straight to the database leaves every reader still seeing the old value until the app pool
// recycles; the repository's own Update evicts the cache as it saves. Doing it the other way here
// would have tested the cache instead of the switch.
var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingMasterRepository>();
var setting = await settingRepo.GetByKey(OutstandingLedger.SettingKey);
if (setting != null)
{
    setting.IsOn = false;
    await settingRepo.Update(setting);

    using var offScope = sp.CreateScope();
    var offOutstanding = offScope.ServiceProvider.GetRequiredService<IOutstandingService>();
    var offCollection = offScope.ServiceProvider.GetRequiredService<ICashCollectionMasterService>();

    Check("the switch reads as off", !await offOutstanding.IsEnabled());
    Check("the existing 25,000 debt is still visible",
        (await offOutstanding.GetOutstanding(Retailer)) == 25_000m,
        $"{await offOutstanding.GetOutstanding(Retailer):F2}");

    string offFail = await offOutstanding.CheckCreditAllowed(Retailer, 1_000m);
    Check("no new credit may be taken", !offFail.EqualsIgnoreCase(StatusType.Ok), offFail);

    var (offCollectFail, _) = await offCollection.AddCashCollection(new AddCashCollectionRequest
    {
        ReceiverID = Retailer, Amount = 1_000m, PayMode = "Cash", Remark = "x", Pin = pin,
    }, Distributor);
    Check("collecting is also blocked while the feature is off",
        !offCollectFail.EqualsIgnoreCase(StatusType.Ok), offCollectFail);

    setting.IsOn = true;
    await settingRepo.Update(setting);
    Check("turning it back on takes effect immediately",
        await scope.ServiceProvider.GetRequiredService<IOutstandingService>().IsEnabled());
}

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n10. A zero credit limit means no credit at all");
// ─────────────────────────────────────────────────────────────────────────────────────────
await outstanding.SetCreditLimit(OtherRetailer, 0m);
string zeroFail = await outstanding.CheckCreditAllowed(OtherRetailer, 1_000m);
Check("a user with a zero limit cannot take credit", !zeroFail.EqualsIgnoreCase(StatusType.Ok), zeroFail);

using (var zeroScope = sp.CreateScope())
{
    var zeroFund = zeroScope.ServiceProvider.GetRequiredService<IFundService>();
    string zeroTransfer = await zeroFund.Transfer(new FundTransferRequest
    {
        ReceiverID = OtherRetailer, WalletID = walletId, Amount = 1_000m,
        Remark = $"verify-zero-{Guid.NewGuid():N}", ModeID = creditModeId, SubModeID = creditSubModeId,
    }, Distributor, "127.0.0.1");
    Check("and a credit transfer to them is refused", !zeroTransfer.EqualsIgnoreCase(StatusType.Ok), zeroTransfer);
    Check("their wallet was not credited", (await Bal(OtherRetailer)) == 0m, $"{await Bal(OtherRetailer):F2}");
}

// ─────────────────────────────────────────────────────────────────────────────────────────
// NOT COVERED HERE: the fund REQUEST path (raise -> approve).
//
// It is the same posting primitive, hooked in AcceptReject, and the harness got as far as
// proving the raise: a 3,00,000 credit request is accepted, and raising it creates no debt
// (correctly - nothing has been given yet). The APPROVAL could not be driven from here, and the
// reason is worth writing down because it is not this feature's:
//
//   FundTransactionRepository.Update does _context.FundTransactions.Update(ft), which paints the
//   WHOLE loaded graph as Modified - including the Sender, Receiver and RequestTo UserMaster rows
//   that ft was loaded with. EF then has to order several UPDATEs against the unique index on
//   UserMasters.MobileNumber and throws "a circular dependency was detected". Nothing in the
//   outstanding code is on that path; approving a fund request has always gone through it.
//
//   It matters beyond the harness: the throw happens AFTER the wallet has moved and after the
//   debt has been posted, and AcceptReject has no try/catch there - so an exception at that line
//   leaves money moved and the request still PROCESS. That is pre-existing, and worth a look on
//   its own rather than a change smuggled into this one.
// ─────────────────────────────────────────────────────────────────────────────────────────

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n12. The statement a distributor opens for one retailer");
// ─────────────────────────────────────────────────────────────────────────────────────────
decimal owedNow = await outstanding.GetOutstanding(Retailer);
var (stmtFail, stmt) = await collection.Statement(new OutstandingStatementRequest
{
    UserID = Retailer, TakeCount = 50,
}, Distributor, UserTypeCode.Distributor);
Check("the distributor can read their retailer's statement", stmtFail.EqualsIgnoreCase(StatusType.Ok), stmtFail);
Check("it reports the live balance", stmt != null && stmt.CurrentOutstanding == owedNow,
    $"statement {stmt?.CurrentOutstanding:F2} vs balance {owedNow:F2}");
Check("it reports how many entries there are in total",
    stmt != null && stmt.TotalCount >= stmt.Rows.Count, $"{stmt?.TotalCount} total, {stmt?.Rows.Count} on this page");
Check("it names both sides of every row",
    stmt != null && stmt.Rows.All(r => r.UserName.Length > 0 && r.ParentName.Length > 0));
Check("credit and debit are separate columns, never both on one row",
    stmt != null && stmt.Rows.All(r => !(r.Credit > 0 && r.Debit > 0)));
Check("every row carries a readable type",
    stmt != null && stmt.Rows.All(r => r.TypeLabel.Length > 0));

var (deniedFail, _) = await collection.Statement(new OutstandingStatementRequest
{
    UserID = Distributor, TakeCount = 50,
}, Retailer, UserTypeCode.Retailer);
Check("a retailer cannot read their upline's statement", !deniedFail.EqualsIgnoreCase(StatusType.Ok), deniedFail);

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n13. The morning reminder sweep");
// ─────────────────────────────────────────────────────────────────────────────────────────
var reminderJob = new FintechApp.Jobs.OutstandingReminderJob(
    sp.GetRequiredService<IServiceScopeFactory>(),
    sp.GetRequiredService<ILogger<FintechApp.Jobs.OutstandingReminderJob>>());

bool swept = true;
string sweepError = "";
try { await reminderJob.SendDailyReminders(); }
catch (Exception ex) { swept = false; sweepError = ex.Message; }
Check("the sweep runs over real data without throwing", swept, sweepError);
Check("the debt it was reminding about is untouched by the sweep",
    (await outstanding.GetOutstanding(Retailer)) == owedNow,
    $"{await outstanding.GetOutstanding(Retailer):F2} vs {owedNow:F2}");

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n14. The network DEFAULT credit limit");
//    Every account starts at zero, which meant no credit until an admin had visited each one.
//    A Key Setting default fixes that - without losing the ability to block one account.
// ─────────────────────────────────────────────────────────────────────────────────────────
await ClearDebt(OtherRetailer);

var settings = scope.ServiceProvider.GetRequiredService<ISettingMasterRepository>();
var defaultRow = await settings.GetByKey("DefaultCreditLimit");
Check("the DefaultCreditLimit setting is seeded", defaultRow != null);
Check("and ships switched OFF - a default must never be turned on by an upgrade",
    defaultRow != null && !defaultRow.IsOn);

Check("with it off, an account with no limit of its own gets no credit",
    !(await outstanding.CheckCreditAllowed(OtherRetailer, 1_000m)).EqualsIgnoreCase(StatusType.Ok));

if (defaultRow != null)
{
    defaultRow.IsOn = true;
    defaultRow.Value = "75000";
    await settings.Update(defaultRow);

    using var defScope = sp.CreateScope();
    var defOutstanding = defScope.ServiceProvider.GetRequiredService<IOutstandingService>();

    Check("the default reads back", (await defOutstanding.GetDefaultCreditLimit()) == 75_000m,
        $"{await defOutstanding.GetDefaultCreditLimit():F2}");

    var standing = await defOutstanding.GetStanding(OtherRetailer);
    Check("an account with no limit of its own now shows the default",
        standing.maxOutstanding == 75_000m && standing.isDefaultLimit,
        $"limit {standing.maxOutstanding:F2}, isDefault {standing.isDefaultLimit}");

    Check("and may take credit up to it",
        (await defOutstanding.CheckCreditAllowed(OtherRetailer, 70_000m)).EqualsIgnoreCase(StatusType.Ok));
    Check("but not past it",
        !(await defOutstanding.CheckCreditAllowed(OtherRetailer, 80_000m)).EqualsIgnoreCase(StatusType.Ok));

    // The distinction the extra column exists for: a deliberate zero is NOT "unconfigured".
    await defOutstanding.SetCreditLimit(OtherRetailer, 0m);
    using (var blockScope = sp.CreateScope())
    {
        var blocked = blockScope.ServiceProvider.GetRequiredService<IOutstandingService>();
        var s2 = await blocked.GetStanding(OtherRetailer);
        Check("an account set to 0 stays blocked even while a default is on",
            s2.maxOutstanding == 0m && !s2.isDefaultLimit,
            $"limit {s2.maxOutstanding:F2}, isDefault {s2.isDefaultLimit}");
        Check("and is refused credit",
            !(await blocked.CheckCreditAllowed(OtherRetailer, 1_000m)).EqualsIgnoreCase(StatusType.Ok));

        // ...and it can be handed back to the default.
        Check("clearing the limit returns it to the default",
            (await blocked.ClearCreditLimit(OtherRetailer)).EqualsIgnoreCase(StatusType.Ok));
    }
    using (var backScope = sp.CreateScope())
    {
        var back = backScope.ServiceProvider.GetRequiredService<IOutstandingService>();
        var s3 = await back.GetStanding(OtherRetailer);
        Check("the account is on the default again", s3.maxOutstanding == 75_000m && s3.isDefaultLimit,
            $"limit {s3.maxOutstanding:F2}, isDefault {s3.isDefaultLimit}");
    }

    defaultRow.IsOn = false;
    defaultRow.Value = "0";
    await settings.Update(defaultRow);
}

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n15. Credit may only be given to a DIRECT child");
//    admin -> dist -> retailer. Admin may fund the retailer normally, but may NOT put credit on
//    them: the debt would be owed to admin while only the distributor can collect cash from them.
// ─────────────────────────────────────────────────────────────────────────────────────────
await ClearDebt(Retailer);
await outstanding.SetCreditLimit(Retailer, 500_000m);

int adminId = await ctx.UserMasters.AsNoTracking()
    .Where(u => u.UserTypeID == UserTypeCode.Admin && u.IsActive)
    .Select(u => u.UserMasterID).FirstOrDefaultAsync();
Check("found the admin account", adminId > 0, $"UserID {adminId}");

var retailerRow = await ctx.UserMasters.AsNoTracking().FirstOrDefaultAsync(u => u.UserMasterID == Retailer);
Check("the retailer reports to the distributor, not to admin",
    retailerRow != null && retailerRow.ParentID == Distributor,
    $"ParentID {retailerRow?.ParentID}");

using (var skipScope = sp.CreateScope())
{
    string skip = await skipScope.ServiceProvider.GetRequiredService<IFundService>().Transfer(new FundTransferRequest
    {
        ReceiverID = Retailer, WalletID = walletId, Amount = 10_000m,
        Remark = $"verify-skiplevel-{Guid.NewGuid():N}",
        ModeID = creditModeId, SubModeID = creditSubModeId,
    }, adminId, "127.0.0.1");

    Check("admin CANNOT put credit on a retailer two levels down", !skip.EqualsIgnoreCase(StatusType.Ok), skip);
    Check("the refusal names who it has to come from instead",
        skip.Contains("reports to", StringComparison.OrdinalIgnoreCase), skip);
    Check("no debt was created", (await outstanding.GetOutstanding(Retailer)) == 0m,
        $"{await outstanding.GetOutstanding(Retailer):F2}");
}

using (var okScope = sp.CreateScope())
{
    string direct = await okScope.ServiceProvider.GetRequiredService<IFundService>().Transfer(new FundTransferRequest
    {
        ReceiverID = Retailer, WalletID = walletId, Amount = 10_000m,
        Remark = $"verify-direct-{Guid.NewGuid():N}",
        ModeID = creditModeId, SubModeID = creditSubModeId,
    }, Distributor, "127.0.0.1");
    Check("their own distributor CAN", direct.EqualsIgnoreCase(StatusType.Ok), direct);
    Check("and the debt is recorded", (await outstanding.GetOutstanding(Retailer)) == 10_000m,
        $"{await outstanding.GetOutstanding(Retailer):F2}");
}

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n16. The statement records which wallet, and what it went from and to");
// ─────────────────────────────────────────────────────────────────────────────────────────
var walletRows = await ccDetails.GetStatement(Retailer, null, null, null, 0, 5);
var top = walletRows.FirstOrDefault();
Check("the credit row names its wallet", top != null && top.WalletID == walletId, $"{top?.WalletID}");
Check("and carries the wallet's opening and closing",
    top?.WalletOpening != null && top?.WalletClosing != null,
    $"{top?.WalletOpening:F2} -> {top?.WalletClosing:F2}");
Check("the wallet moved by the credit amount",
    top?.WalletClosing - top?.WalletOpening == 10_000m,
    $"{(top?.WalletClosing - top?.WalletOpening):F2}");

// A collection moves no wallet, so those stay null rather than a misleading 0.00.
int? collectorPin = await ctx.UserMasters.AsNoTracking()
    .Where(u => u.UserMasterID == Distributor).Select(u => u.Mpin).FirstOrDefaultAsync();
var (collFail2, _) = await collection.AddCashCollection(new AddCashCollectionRequest
{
    ReceiverID = Retailer, Amount = 5_000m, PayMode = "Cash", Remark = "wallet-null check",
    Pin = (collectorPin ?? 0).ToString(),
}, Distributor);
Check("a collection is recorded", collFail2.EqualsIgnoreCase(StatusType.Ok), collFail2);

var collRow = (await ccDetails.GetStatement(Retailer, null, null, null, 0, 1)).FirstOrDefault();
Check("and leaves the wallet columns NULL, not 0.00",
    collRow != null && collRow.WalletOpening == null && collRow.WalletClosing == null,
    $"{collRow?.WalletOpening?.ToString() ?? "null"} / {collRow?.WalletClosing?.ToString() ?? "null"}");

// ─────────────────────────────────────────────────────────────────────────────────────────
Console.WriteLine("\n17. Every role can read its OWN standing");
// ─────────────────────────────────────────────────────────────────────────────────────────
var (ownFail, own) = await collection.SelectUserOutstanding(Retailer);
Check("the retailer's own standing reads", ownFail.EqualsIgnoreCase(StatusType.Ok), ownFail);
Check("it reports what is owed", own != null && own.Outstanding == 5_000m, $"{own?.Outstanding:F2}");
Check("it names who to pay", own != null && own.ParentName.Length > 0, own?.ParentName ?? "");
Check("it reports the headroom", own != null && own.AvailableCredit == 495_000m, $"{own?.AvailableCredit:F2}");
Check("it lists the real wallet balances (not the empty legacy table)",
    own != null && own.BalanceDetail.Length > 0, own?.BalanceDetail ?? "");

Console.WriteLine($"\n{passed} passed, {failed} failed.\n");
return failed == 0 ? 0 : 1;

async Task ClearDebt(int userId)
{
    await ctx.Database.ExecuteSqlRawAsync("DELETE FROM CashCollectionDetails WHERE UserID = {0}", userId);
    await ctx.Database.ExecuteSqlRawAsync("DELETE FROM CashCollectionMasters WHERE UserID = {0}", userId);
}

async Task<decimal> Bal(int userId)
{
    var w = await walletRepo.GetByUserAndWallet(userId, walletId);
    return w?.Balance ?? 0m;
}

/// <summary>
/// A message sender that sends nothing. These flows text the retailer on every movement, and a
/// verification run against a copy of live data must not put a real SMS on a real phone.
/// </summary>
sealed class SilentMessages : IMessageService
{
    public Task SendEventAsync(string eventType, int userId, string mobile, string whatsAppNumber,
        string emailId, IDictionary<string, string> tokens, string fallbackSms,
        bool isOtpDefault = false, string emailSubject = "") => Task.CompletedTask;
    public Task SendSmsAsync(int userId, string mobile, string message, string templateId, string type) => Task.CompletedTask;
    public Task SendOtpSmsAsync(int userId, string mobile, string message, string templateId, string type) => Task.CompletedTask;
    public Task SendWhatsAppAsync(int userId, string mobile, string whatsAppNumber, string message, string templateId, string type) => Task.CompletedTask;
    public Task SendEmailAsync(string emailId, string message, string subject, string attachment = "") => Task.CompletedTask;
    public Task<string> CheckSmsAsync(string mobile, string message, string templateId) => Task.FromResult("OK");
    public Task<string> CheckEmailAsync(string emailId, string message, string subject) => Task.FromResult("OK");
    public Task<string> CheckWhatsAppAsync(string whatsAppNumber, string message) => Task.FromResult("OK");
}
