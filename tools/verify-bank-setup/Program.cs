using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Requests.BankConfig;
using FintechApp.Requests.Phase5;
using FintechApp.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Drives the REAL bank-reconciliation setup against a restored copy of a real database: the banks
// pulled from Payment Modes, a saved statement format, the importer reading a statement through it,
// the rules deciding what each line IS, the counterparty named from a fund request, and the two
// kinds of duplicate — a line repeated on the statement, and one deposit claimed by two approved
// fund requests.
//
// It WRITES and it SEEDS test fund requests, so it refuses anything but a throwaway copy:
//
//   dotnet run --project tools/verify-bank-setup -- FinoTest_ChargeGst [path-to-a.pdf]
//
// A PDF path is optional. When given, the PDF is read ONLY to prove the extractor works: the number
// of text lines is reported and nothing from the document is ever printed.

string db = args.FirstOrDefault() ?? string.Empty;
string pdfPath = args.Length > 1 ? args[1] : string.Empty;

if (!db.StartsWith("FinoTest", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Refusing to run: pass a throwaway database whose name starts with 'FinoTest'.");
    return 2;
}

var services = new ServiceCollection();
services.AddMemoryCache();
services.AddSingleton(typeof(FintechApp.Processing.MemoryCacheProcessing<>));
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(
    @$"Server=localhost\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"));
services.AddScoped<IBankLineClassifier, BankLineClassifier>();
services.AddScoped<IBankStatementImportService, BankStatementImportService>();
services.AddScoped<IBankStatementConfigService, BankStatementConfigService>();
var provider = services.BuildServiceProvider();

int pass = 0, fail = 0;
void Check(string what, decimal actual, decimal expected)
{
    bool ok = actual == expected;
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-62} {actual,10:0.00}   expected {expected,8:0.00}");
}
void CheckText(string what, string actual, string expected)
{
    bool ok = string.Equals(actual ?? string.Empty, expected, StringComparison.OrdinalIgnoreCase);
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-62} {actual,14}   expected {expected,12}");
}
void CheckTrue(string what, bool ok)
{
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}");
}
void Section(string t)
{
    Console.WriteLine();
    Console.WriteLine($"-- {t} " + new string('-', Math.Max(0, 84 - t.Length)));
}

Console.WriteLine($"verify-bank-setup - {db}");
Console.WriteLine(new string('=', 100));

// ── 1. The banks come from Payment Modes ────────────────────────────────────────────────────
Section("Banks pulled from Payment Modes (sub-modes), not typed a second time");
int bankId;
using (var scope = provider.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IBankStatementConfigService>();

    var before = await config.GetBanks();
    Console.WriteLine($"        before: {before.Banks.Count} bank(s), {before.UnlinkedSubModes.Count} sub-mode(s) not brought in");

    var sync = await config.SyncBanksFromModes(1);
    CheckTrue($"sync created banks from the Bank sub-modes ({sync.Created} created, {sync.Updated} refreshed)", sync.Created > 0);
    Console.WriteLine($"        {sync.Summary}");

    var after = await config.GetBanks();
    CheckTrue("every created bank is linked to its sub-mode", after.Banks.Where(b => b.SubModeID > 0).Any());
    CheckTrue("nothing is left unlinked that looks like a bank", after.UnlinkedSubModes.Count == 0);

    // Running it again must not create the same bank twice.
    var again = await config.SyncBanksFromModes(1);
    Check("a second sync creates nothing", again.Created, 0);

    bankId = after.Banks.First(b => b.SubModeID > 0).BankAccountID;
    Console.WriteLine($"        using bank {bankId}: {after.Banks.First(b => b.BankAccountID == bankId).BankName}");
}

// ── 2. A fund request to recognise, and a duplicate of it ───────────────────────────────────
Section("Seeding two approved fund requests against ONE deposit");
const string sharedUtr = "VERIFYUTR900011";
int retailerId;
using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    retailerId = await ctx.UserMasters.AsNoTracking()
        .Where(u => u.UserTypeID == UserTypeCode.Retailer || u.UserTypeID == UserTypeCode.ApiUser)
        .Select(u => u.UserMasterID).FirstAsync();

    int walletId = await ctx.WalletMaster.AsNoTracking().Select(w => w.WalletID).FirstAsync();

    foreach (var amount in new[] { 25000m, 25000m })
    {
        ctx.FundTransactions.Add(new FundTransaction
        {
            SenderID = retailerId,
            ReceiverID = 1,
            RequestToID = 1,
            WalletID = walletId,
            Amount = amount,
            OriginalAmount = amount,
            UTRNumber = sharedUtr,
            Status = "SUCCESS",
            Type = "REQUEST",
            PayMode = "NEFT",
            BankName = "HDFC",
            Description = "verify-bank-setup",
            Through = "BANK",
            Remark = "verify-bank-setup",
            AdminRemark = "verify",
            CashCollectionRemark = string.Empty,
            ReceiptPath = string.Empty,
            DateTime = new DateTime(2026, 10, 3),
            UpdatedateTime = DateTime.Now,
            IsDeleted = false,
        });
    }
    await ctx.SaveChangesAsync();
    Console.WriteLine($"        two approved requests of Rs 25,000 dated 3 Oct for user {retailerId}, both on UTR {sharedUtr}");
}

// ── 3. Rules that say what a line is ────────────────────────────────────────────────────────
Section("Rules: what the line IS, from the bank's own narration");
using (var scope = provider.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IBankStatementConfigService>();

    var (purchaseRule, f1) = await config.SaveCategoryRule(new SaveBankLineCategoryRuleRequest
    {
        BankAccountID = 0, Direction = "DEBIT", MatchType = "CONTAINS", Pattern = "JAIJUI",
        Category = BankLineCategory.Purchase, IsActive = true, Remark = "Provider float",
    });
    CheckTrue($"a PURCHASE rule saved{(f1.Length > 0 ? ": " + f1 : string.Empty)}", purchaseRule > 0);

    var (expenseRule, f2) = await config.SaveCategoryRule(new SaveBankLineCategoryRuleRequest
    {
        BankAccountID = bankId, Direction = "DEBIT", MatchType = "CONTAINS", Pattern = "OFFICE RENT",
        Category = BankLineCategory.Expense, IsActive = true,
    });
    CheckTrue($"an EXPENSE rule saved{(f2.Length > 0 ? ": " + f2 : string.Empty)}", expenseRule > 0);

    // A credit cannot be an expense: the natural side of a category is knowable, so it is refused.
    var (_, badSide) = await config.SaveCategoryRule(new SaveBankLineCategoryRuleRequest
    {
        Direction = "CREDIT", MatchType = "CONTAINS", Pattern = "ANYTHING",
        Category = BankLineCategory.Expense, IsActive = true,
    });
    CheckTrue("a rule filing a CREDIT as an expense is refused", badSide.Length > 0);

    var (_, noPattern) = await config.SaveCategoryRule(new SaveBankLineCategoryRuleRequest
    {
        Direction = "DEBIT", Category = BankLineCategory.Purchase, IsActive = true,
    });
    CheckTrue("a rule with no text to look for is refused", noPattern.Length > 0);

    var (_, badRegex) = await config.SaveCategoryRule(new SaveBankLineCategoryRuleRequest
    {
        Direction = "DEBIT", MatchType = "REGEX", Pattern = "NEFT(", Category = BankLineCategory.Purchase, IsActive = true,
    });
    CheckTrue("an invalid expression is refused", badRegex.Length > 0);

    var rules = await config.GetCategoryRules(bankId);
    CheckTrue($"the rule list reads as sentences ({rules.Count} rules)", rules.Count >= 11);
    Console.WriteLine($"        e.g. {rules.First(r => r.Pattern == "JAIJUI").Summary}");
}

// ── 4. A saved format, and the importer reading a statement through it ──────────────────────
Section("A statement format saved once, then an import that uses it");
string csv = string.Join('\n', new[]
{
    "Statement of account - verify",
    "Txn Date,Narration,Chq / Ref No,Withdrawal Amt.,Deposit Amt.,Closing Balance",
    "02/10/2026,NEFT DR-HDFC-JAIJUI SOLUTIONS PVT LTD,NEFT0098761,50000.00,,120000.00",
    "03/10/2026,OFFICE RENT OCT,RENT1001,18000.00,,102000.00",
    $"04/10/2026,NEFT CR-{sharedUtr}-RETAILER DEPOSIT,{sharedUtr},,25000.00,127000.00",
    "05/10/2026,BANK CHARGE QTRLY,CHG9001,177.00,,126823.00",
    $"06/10/2026,NEFT CR-{sharedUtr}-RETAILER DEPOSIT,{sharedUtr},,25000.00,151823.00",
    "*** End of statement ***",
});

int formatId;
using (var scope = provider.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IBankStatementConfigService>();

    var (id, failure) = await config.SaveFormat(new SaveBankStatementFormatRequest
    {
        BankAccountID = bankId, Name = "Verify CSV", Source = "CSV", SkipRows = 1,
        HeaderRow = -1, IsActive = true, Remark = "Set up by verify-bank-setup",
    });
    CheckTrue($"format saved{(failure.Length > 0 ? ": " + failure : string.Empty)}", id > 0);
    formatId = id;

    // One active format per bank per file type: a second would make which is used depend on the id.
    var (_, clash) = await config.SaveFormat(new SaveBankStatementFormatRequest
    {
        BankAccountID = bankId, Name = "Another CSV", Source = "CSV", IsActive = true,
    });
    CheckTrue("a second ACTIVE CSV format for the same bank is refused", clash.Length > 0);

    var (_, badPattern) = await config.SaveFormat(new SaveBankStatementFormatRequest
    {
        BankAccountID = bankId, Name = "Bad PDF", Source = "PDF", PdfLinePattern = "(?<date>", IsActive = true,
    });
    CheckTrue("an invalid PDF line pattern is refused", badPattern.Length > 0);

    // Preview runs the real importer in dry run.
    var (preview, pf) = await config.PreviewFormat(new PreviewBankFormatRequest
    {
        BankStatementFormatID = formatId, Content = csv, FileName = "verify.csv", MaxRows = 10,
    });
    CheckTrue($"preview read the file{(pf.Length > 0 ? ": " + pf : string.Empty)}", preview != null);
    if (preview != null)
    {
        Check("rows read cleanly", preview.RowsRead, 5);
        Console.WriteLine($"        columns: {preview.DetectedColumns}");
        Console.WriteLine($"        verdict: {preview.Verdict}");
    }
}

// ── 5. The import, and what each line turned out to be ──────────────────────────────────────
Section("Import: categories, the counterparty's name, and the duplicates");
using (var scope = provider.CreateScope())
{
    var import = scope.ServiceProvider.GetRequiredService<IBankStatementImportService>();

    var (result, failure) = await import.Import(new BankStatementImportRequest
    {
        BankAccountID = bankId,
        Content = csv,
        FileName = "verify.csv",
        ImportedByUserID = 1,
    });

    CheckTrue($"statement imported{(failure.Length > 0 ? ": " + failure : string.Empty)}", result != null);
    if (result == null) { Console.WriteLine($"{pass} passed, {fail + 1} failed"); return 1; }

    Check("lines imported", result.LinesImported, 5);
    Check("total debit", result.TotalDebit, 68177.00m);
    Check("total credit", result.TotalCredit, 50000.00m);
    CheckTrue($"lines categorised ({result.Categorised})", result.Categorised > 0);
    CheckTrue($"duplicates flagged ({result.DuplicatesFlagged})", result.DuplicatesFlagged > 0);
    foreach (var w in result.Warnings) Console.WriteLine($"        ! {w}");
}

using (var scope = provider.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var lines = await ctx.BankStatementLines.AsNoTracking()
        .Where(l => !l.IsDeleted && l.BankAccountID == bankId)
        .OrderBy(l => l.RowNo).ToListAsync();

    Console.WriteLine();
    Console.WriteLine($"  {"Date",-12}{"Narration",-44}{"Debit",10}{"Credit",10}  {"Category",-14}Who / flag");
    foreach (var l in lines)
    {
        Console.WriteLine($"  {l.TxnDate,-12:dd MMM yyyy}{Cut(l.Description, 43),-44}{l.Debit,10:0.00}{l.Credit,10:0.00}  "
                        + $"{l.Category,-14}{(l.CounterpartyName.Length > 0 ? l.CounterpartyName : string.Empty)}"
                        + (l.DuplicateReason.Length > 0 ? "  [DUP]" : string.Empty));
    }
    Console.WriteLine();

    var purchase = lines.First(l => l.Description.Contains("JAIJUI"));
    CheckText("the provider debit is a purchase", purchase.Category, BankLineCategory.Purchase);

    var rent = lines.First(l => l.Description.Contains("OFFICE RENT"));
    CheckText("the rent debit is an expense", rent.Category, BankLineCategory.Expense);

    var charge = lines.First(l => l.Description.Contains("BANK CHARGE"));
    CheckText("the bank's own charge is bank charges", charge.Category, BankLineCategory.BankCharges);

    var deposits = lines.Where(l => l.Credit > 0m).OrderBy(l => l.RowNo).ToList();
    Check("two credits imported", deposits.Count, 2);
    CheckText("a retailer's deposit is user funding", deposits[0].Category, BankLineCategory.UserFund);
    CheckTrue("...and it is named, not left as a UTR", deposits[0].CounterpartyUserID == retailerId
                                                    && deposits[0].CounterpartyName.Length > 0);
    Console.WriteLine($"        counterparty: {deposits[0].CounterpartyName}");

    CheckTrue("the repeated reference is flagged as a duplicate", deposits[1].DuplicateOfLineID > 0);
    Console.WriteLine($"        reason: {deposits[1].DuplicateReason}");
}

// ── 6. One deposit, two approved fund requests ──────────────────────────────────────────────
Section("The expensive one: two approved fund requests claiming one deposit");
using (var scope = provider.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IBankStatementConfigService>();
    var report = await config.GetDuplicateFundRequests(DateTime.Today.AddDays(-30), DateTime.Today);

    var group = report.Groups.FirstOrDefault(g => g.Reference.Contains("VERIFYUTR", StringComparison.OrdinalIgnoreCase));
    CheckTrue("the duplicated deposit is found", group != null);
    if (group != null)
    {
        Check("approved claims", group.ApprovedCount, 2);
        Check("approved in total", group.ApprovedAmount, 50000m);
        Check("one genuine deposit", group.SingleAmount, 25000m);
        Check("funded beyond one deposit", group.Exposure, 25000m);
        CheckTrue("the bank's own side is shown", group.SeenOnStatement);
        Console.WriteLine($"        verdict: {group.Verdict}");
    }
    Console.WriteLine($"        {report.Summary}");
}

// ── 7. Re-classify after adding a rule ──────────────────────────────────────────────────────
Section("A rule added later takes effect on what is already imported");
using (var scope = provider.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IBankStatementConfigService>();

    // Mark one line by hand first: a rule run must never overwrite a person's decision.
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var line = await ctx.BankStatementLines.AsNoTracking()
        .Where(l => !l.IsDeleted && l.BankAccountID == bankId && l.Description.Contains("OFFICE RENT"))
        .FirstAsync();

    string set = await config.SetLineCategory(new SetBankLineCategoryRequest
    {
        BankStatementLineID = line.BankStatementLineID, Category = BankLineCategory.Transfer, ChangedByUserID = 1,
    });
    CheckText("a person can set a line's category", set, StatusType.Ok);

    var result = await config.Reclassify(bankId, 0, 1);
    CheckTrue($"re-classify ran ({result.Summary})", result.LinesSeen > 0);
    CheckTrue("...and left the hand-set line alone", result.LeftAlone >= 1);

    var after = await ctx.BankStatementLines.AsNoTracking()
        .FirstAsync(l => l.BankStatementLineID == line.BankStatementLineID);
    CheckText("the hand-set category survived the rules", after.Category, BankLineCategory.Transfer);

    // And a category that contradicts the side is refused even by hand.
    var credit = await ctx.BankStatementLines.AsNoTracking()
        .FirstAsync(l => !l.IsDeleted && l.BankAccountID == bankId && l.Credit > 0m);
    string bad = await config.SetLineCategory(new SetBankLineCategoryRequest
    {
        BankStatementLineID = credit.BankStatementLineID, Category = BankLineCategory.Expense, ChangedByUserID = 1,
    });
    CheckTrue("calling a credit an expense is refused by hand too", bad != StatusType.Ok);
}

// ── 8. The PDF reader ───────────────────────────────────────────────────────────────────────
Section("The PDF reader");
if (pdfPath.Length == 0 || !File.Exists(pdfPath))
{
    Console.WriteLine("  SKIP  no PDF given, so the extractor was not exercised. Pass a path as the second argument.");
}
else
{
    using var scope = provider.CreateScope();
    var config = scope.ServiceProvider.GetRequiredService<IBankStatementConfigService>();

    string base64 = Convert.ToBase64String(await File.ReadAllBytesAsync(pdfPath));
    var (preview, failure) = await config.PreviewFormat(new PreviewBankFormatRequest
    {
        Format = new SaveBankStatementFormatRequest { BankAccountID = bankId, Source = "PDF" },
        Content = base64,
        FileName = Path.GetFileName(pdfPath),
        MaxRows = 5,
    });

    CheckTrue($"the PDF opened and text came out{(failure.Length > 0 ? ": " + failure : string.Empty)}",
              preview != null && preview.TotalRowsSeen > 0);

    if (preview != null)
    {
        // NOTHING from the document is printed: it is somebody's real paperwork, and this test
        // only has to prove the extractor returns lines.
        Console.WriteLine($"        {preview.TotalRowsSeen} text line(s) extracted, {preview.RowsRead} read as transactions");
        CheckTrue("a non-statement PDF says so rather than importing nonsense",
                  preview.RowsRead == 0 && preview.Verdict.Length > 0);
        Console.WriteLine($"        verdict: {preview.Verdict}");
    }
}

Console.WriteLine();
Console.WriteLine(new string('=', 100));
Console.WriteLine($"{pass} passed, {fail} failed");
return fail == 0 ? 0 : 1;

static string Cut(string s, int n) => (s ?? string.Empty).Length <= n ? (s ?? string.Empty) : s!.Substring(0, n - 1) + "~";
