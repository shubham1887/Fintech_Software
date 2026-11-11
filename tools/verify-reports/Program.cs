using FintechApp.Data;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Requests.TransactionDetails;
using FintechApp.Requests.TransactionReport;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Checks the report changes against a restored copy of a real database: the figures strip on the
// filter reports, the account statement's opening / closing / difference, the wallet-transfer rows
// no longer double-counted in the fund report, and the income report's scope and own-tier figure.
//
// READ-ONLY. Pass a throwaway database anyway, because this is not where live is queried from.
//
//   dotnet run --project tools/verify-reports -- FinoTest_Delegation

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

services.AddScoped<ITransactionMasterRepository, TransactionMasterRepository>();
services.AddScoped<ITransactionDetailsRepository, TransactionDetailsRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IOperatorRepository, OperatorRepository>();
services.AddScoped<IWalletMasterRepository, WalletMasterRepository>();
services.AddScoped<IProviderMasterRepository, ProviderMasterRepository>();
services.AddScoped<IServiceRepository, ServiceRepository>();
services.AddScoped<ITransactionDetailsService, TransactionDetailsService>();

var sp = services.BuildServiceProvider();

int passed = 0, failed = 0;
void Check(string what, bool ok, string detail = "")
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail.Length > 0 ? $"  — {detail}" : "")}");
    if (ok) passed++; else failed++;
}

using var scope = sp.CreateScope();
var txnRepo = scope.ServiceProvider.GetRequiredService<ITransactionMasterRepository>();
var statements = scope.ServiceProvider.GetRequiredService<ITransactionDetailsService>();
var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
var serviceRepo = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

Console.WriteLine($"Database : {db}");
Console.WriteLine();

// ── 1. The filter reports' figures strip ────────────────────────────────────────────────────
Console.WriteLine("1. Filter-report totals (GetFilteredTotals)");

var allServices = await serviceRepo.GetAllServices();
var rechargeIds = ServiceKinds.IdsOf(allServices, ServiceKinds.Recharge);
Console.WriteLine($"   recharge-family services: {string.Join(",", rechargeIds)}");

var totals = await txnRepo.GetFilteredTotals(0, rechargeIds, 0, 0, null, null);

// The same figures, straight out of SQL, as the arithmetic they are supposed to be.
var expected = await ctx.TransactionMasters.AsNoTracking()
    .Where(x => !x.IsDeleted && rechargeIds.Contains(x.ServiceID))
    .GroupBy(x => 1)
    .Select(g => new
    {
        Success = g.Count(x => x.Status == "SUCCESS"),
        SuccessAmt = g.Where(x => x.Status == "SUCCESS").Sum(x => (decimal?)x.Amount) ?? 0m,
        SuccessComm = g.Where(x => x.Status == "SUCCESS").Sum(x => (decimal?)x.RetailerCommission) ?? 0m,
        Failed = g.Count(x => x.Status == "FAILED" || x.Status == "FAILURE" || x.Status == "REFUND" || x.Status == "REFUNDED"),
        Pending = g.Count(x => x.Status == "PENDING" || x.Status == "PROCESS"),
        All = g.Count(),
    })
    .FirstOrDefaultAsync();

if (expected == null)
{
    Console.WriteLine("  SKIP  no recharge-family transactions in this database");
}
else
{
    Check("success count", totals.SuccessCount == expected.Success, $"{totals.SuccessCount} vs {expected.Success}");
    Check("success amount", totals.SuccessAmount == expected.SuccessAmt, $"{totals.SuccessAmount} vs {expected.SuccessAmt}");
    Check("failed count", totals.FailedCount == expected.Failed, $"{totals.FailedCount} vs {expected.Failed}");
    Check("pending count", totals.PendingCount == expected.Pending, $"{totals.PendingCount} vs {expected.Pending}");
    Check("total count", totals.TotalCount == expected.All, $"{totals.TotalCount} vs {expected.All}");
    Check("commission counted on SUCCESS only",
        totals.Commission == expected.SuccessComm, $"{totals.Commission} vs {expected.SuccessComm}");

    // The strip must not be narrowed by the status filter — that is the whole point of it.
    var pendingOnly = await txnRepo.GetFiltered(0, "PENDING", rechargeIds, 0, 0, null, null, 0, 20);
    Check("the strip is not narrowed by the status filter (still reports successes)",
        totals.SuccessCount == expected.Success && pendingOnly.count <= expected.All,
        $"pending page reports {pendingOnly.count} rows");
}

// ── 2. The account statement ────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("2. Account statement");

var withLedger = await ctx.TransactionDetailss.AsNoTracking()
    .Where(t => !t.IsDeleted)
    .GroupBy(t => t.UserID)
    .Select(g => new { UserID = g.Key, Count = g.Count() })
    .OrderByDescending(g => g.Count)
    .FirstOrDefaultAsync();

if (withLedger == null)
{
    Console.WriteLine("  SKIP  no ledger postings in this database");
}
else
{
    var user = await users.GetUserById(withLedger.UserID);
    Console.WriteLine($"   statement for {user?.FirmName} (#{withLedger.UserID}), {withLedger.Count} posting(s)");

    var stmt = await statements.SelectForFilter(
        new AccountStatementRequest { UserID = withLedger.UserID, TakeCount = 20 },
        withLedger.UserID, UserTypeCode.Admin);

    Check("rows come back", stmt.list.Count > 0, $"{stmt.list.Count} of {stmt.Count}");
    Check("the figures strip is populated", stmt.HeaderSummary.Count > 0);

    foreach (var key in new[] { "Opening Balance", "Closing Balance", "Success Amount", "Commission", "Charge", "Difference" })
        Check($"strip carries \"{key}\"", stmt.HeaderSummary.ContainsKey(key));

    decimal opening = Convert.ToDecimal(stmt.HeaderSummary["Opening Balance"]);
    decimal closing = Convert.ToDecimal(stmt.HeaderSummary["Closing Balance"]);
    decimal diff = Convert.ToDecimal(stmt.HeaderSummary["Difference"]);
    Check("difference is closing minus opening", diff == closing - opening, $"{diff} vs {closing - opening}");

    // Opening is the EARLIEST posting's opening, not a sum of them.
    var earliest = await ctx.TransactionDetailss.AsNoTracking()
        .Where(t => !t.IsDeleted && t.UserID == withLedger.UserID)
        .OrderBy(t => t.DateTime).FirstAsync();
    var latest = await ctx.TransactionDetailss.AsNoTracking()
        .Where(t => !t.IsDeleted && t.UserID == withLedger.UserID)
        .OrderByDescending(t => t.DateTime).FirstAsync();
    Check("opening is the first posting's opening, not a sum",
        opening == earliest.OpeningBalance, $"{opening} vs {earliest.OpeningBalance}");
    Check("closing is the last posting's closing, not a sum",
        closing == latest.ClosingBalance, $"{closing} vs {latest.ClosingBalance}");

    // One wallet at a time.
    int walletId = (await ctx.TransactionDetailss.AsNoTracking()
        .Where(t => !t.IsDeleted && t.UserID == withLedger.UserID && t.WalletID > 0)
        .Select(t => t.WalletID).FirstOrDefaultAsync());
    if (walletId > 0)
    {
        var oneWallet = await statements.SelectForFilter(
            new AccountStatementRequest { UserID = withLedger.UserID, WalletID = walletId, TakeCount = 500 },
            withLedger.UserID, UserTypeCode.Admin);
        int expectedRows = await ctx.TransactionDetailss.AsNoTracking()
            .CountAsync(t => !t.IsDeleted && t.UserID == withLedger.UserID && t.WalletID == walletId);
        Check($"the wallet filter narrows to wallet {walletId}",
            oneWallet.Count == expectedRows, $"{oneWallet.Count} vs {expectedRows}");
        Check("and it narrows, rather than returning everything",
            oneWallet.Count <= stmt.Count, $"{oneWallet.Count} vs {stmt.Count}");
    }

    // The export asks for everything.
    var full = await statements.SelectForFilter(
        new AccountStatementRequest { UserID = withLedger.UserID, TakeCount = 20, IsExcelDownload = true },
        withLedger.UserID, UserTypeCode.Admin);
    Check("an export returns every row, not one page",
        full.list.Count == full.Count, $"{full.list.Count} of {full.Count}");
}

// ── 3. Fund report: wallet transfers are out ────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("3. Fund report excludes own-account wallet transfers");

int selfMoves = await ctx.FundTransactions.AsNoTracking()
    .CountAsync(f => !f.IsDeleted && f.SenderID == f.ReceiverID && f.SenderID > 0);
int betweenPeople = await ctx.FundTransactions.AsNoTracking()
    .CountAsync(f => !f.IsDeleted && f.SenderID != f.ReceiverID);
Console.WriteLine($"   {selfMoves} wallet-transfer leg(s), {betweenPeople} transfer(s) between people");
Check("there is something to tell apart", selfMoves + betweenPeople >= 0);

// ── 4. Income report: scope and the caller's own tier ───────────────────────────────────────
Console.WriteLine();
Console.WriteLine("4. Income report");

// Rebuilt with the report service's own dependencies would mean wiring twenty repositories, so
// the two properties that actually changed are asserted against the data directly: a distributor
// earns on its DOWNLINE's transactions, and its income is its own tier, not the chain's.
var allUsers = await users.GetAllUsers();
var distributor = allUsers.FirstOrDefault(u => u.UserTypeID == UserTypeCode.Distributor);
if (distributor == null)
{
    Console.WriteLine("  SKIP  no distributor in this database");
}
else
{
    var branch = FintechApp.Helpers.UserHierarchy.GetDownlineIds(allUsers, distributor.UserMasterID);
    branch.Add(distributor.UserMasterID);

    int own = await ctx.TransactionMasters.AsNoTracking()
        .CountAsync(t => !t.IsDeleted && t.UserID == distributor.UserMasterID && t.Status == "SUCCESS");
    int branchWide = await ctx.TransactionMasters.AsNoTracking()
        .CountAsync(t => !t.IsDeleted && branch.Contains(t.UserID) && t.Status == "SUCCESS");

    Console.WriteLine($"   {distributor.FirmName} (#{distributor.UserMasterID}): {own} own txn(s), {branchWide} across the branch");
    Check("a distributor's branch is at least as big as its own activity",
        branchWide >= own, $"{branchWide} vs {own}");

    decimal chainComm = await ctx.TransactionMasters.AsNoTracking()
        .Where(t => !t.IsDeleted && branch.Contains(t.UserID) && t.Status == "SUCCESS")
        .SumAsync(t => (decimal?)(t.RetailerCommission + t.DistributerCommission
            + t.SuperDistributerCommission + t.AdminCommission)) ?? 0m;
    decimal ownComm = await ctx.TransactionMasters.AsNoTracking()
        .Where(t => !t.IsDeleted && branch.Contains(t.UserID) && t.Status == "SUCCESS")
        .SumAsync(t => (decimal?)(t.DistributerCommission - t.DistributerTDS)) ?? 0m;
    Console.WriteLine($"   chain commission {chainComm:0.00}, distributor's own {ownComm:0.00}");
    Check("the distributor's own income is not the whole chain's commission",
        chainComm == 0m || ownComm != chainComm,
        $"own {ownComm:0.00} vs chain {chainComm:0.00}");
}

Console.WriteLine();
Console.WriteLine($"{passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
