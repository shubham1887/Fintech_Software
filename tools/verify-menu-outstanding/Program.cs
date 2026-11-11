// verify-menu-outstanding — drives the three things this change added, over HTTP, as real users.
//
// What it asserts, and why each one is worth a test rather than a look:
//
//   • The outstanding report's AGEING. The four buckets have to total the balance exactly, for
//     every row and for the report. A bucket split that quietly loses or double-counts a rupee
//     is the kind of error nobody spots on screen — the page still looks like a report — and it
//     is read as a statement of what a business owes.
//
//   • The report's SCOPE. Admin sees the network; a retailer sees their own position and no
//     downline at all. The server decides this from the token, so it is checked from two
//     different tokens rather than from one and a parameter.
//
//   • MENU VISIBILITY end to end: hide an entry, see it leave the sidebar, restore it, see it
//     come back — on the live menu endpoint every user loads, not on the admin screen's own
//     read. The cache between them is exactly where this feature would fail silently.
//
//   • MONTH-END INVOICING: that it REFUSES a month whose parties have no place of supply, that
//     it issues when they do, and that running it a second time issues nothing. The last one is
//     what makes it safe to put on a schedule at all — a month-end job that double-invoices
//     every time it runs would be the most expensive kind of automation.
//
// It WRITES (it hides and restores a menu row, and it issues documents), so it refuses anything
// but a throwaway database.
//
//   dotnet run --project tools/verify-menu-outstanding -- <baseUrl> FinoTest_Whatever <settingsFolder>

using System.Net.Http.Headers;
using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

string baseUrl = (args.ElementAtOrDefault(0) ?? "http://127.0.0.1:5099").TrimEnd('/');
string db = args.ElementAtOrDefault(1) ?? "";
string settingsDir = args.ElementAtOrDefault(2) ?? @"C:\Users\Administrator\Desktop\Fintech_Software\Fino_API";

if (!db.StartsWith("FinoTest", StringComparison.OrdinalIgnoreCase)
 && !db.Contains("claudetest", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Refusing to run: this writes. Pass a throwaway database.");
    return 2;
}

var config = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(settingsDir, "appsettings.json"), optional: false)
    .AddJsonFile(Path.Combine(settingsDir, "appsettings.Production.json"), optional: true)
    .Build();
var jwt = new JwtTokenService(config);
var crypto = new EncryptDecrypt(config["Encryption:Key"] ?? "", config["Encryption:IV"] ?? "");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer($"Server=localhost\\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;")
    .Options;
await using var ctx = new ApplicationDbContext(options);

var admin = await ctx.UserMasters.AsNoTracking().FirstOrDefaultAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Admin);
var retailer = await ctx.UserMasters.AsNoTracking().FirstOrDefaultAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Retailer);
if (admin == null) { Console.WriteLine($"No admin in {db}."); return 3; }

int passed = 0, failed = 0;
void Check(string what, bool ok, string detail = "")
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail.Length > 0 ? $"  — {detail}" : "")}");
    if (ok) passed++; else failed++;
}

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };

// One caller = one token + one matching envelope. They must name the same user or the API
// refuses the request, which is the check that stops one signed-in user acting as another.
(Func<string, object, Task<JToken>> post, string name) As(int userId, int typeId, string typeName,
    string mobile, string email, int epoch)
{
    var tokens = jwt.GenerateJwtToken(userId, typeId, typeName, mobile, email, epoch);
    string keyData = crypto.Encrypt(JsonConvert.SerializeObject(new
    {
        UserID = userId, UserTypeID = typeId, SessionToken = tokens.AccessToken,
        DeviceType = "Web", IPAddress = "127.0.0.1",
    }));

    async Task<JToken> Post(string path, object body)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent(crypto.Encrypt(JsonConvert.SerializeObject(body))), "Request" },
            { new StringContent(keyData), "KeyData" },
        };
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/{path}") { Content = form };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var res = await http.SendAsync(req);
        var envelope = JObject.Parse(await res.Content.ReadAsStringAsync());

        string status = envelope["Status"]?.ToString() ?? envelope["status"]?.ToString() ?? "";
        if (!status.Equals("Success", StringComparison.OrdinalIgnoreCase))
            throw new Exception($"{path}: HTTP {(int)res.StatusCode} {envelope.ToString(Formatting.None)}");

        string payload = envelope["data"]?.ToString() ?? envelope["Data"]?.ToString() ?? "";
        if (payload.Length == 0) return new JObject();
        return Parse(payload) ?? Parse(crypto.Decrypt(payload)) ?? throw new Exception($"{path}: unreadable payload.");

        static JToken? Parse(string s)
        {
            s = (s ?? "").TrimStart();
            try
            {
                if (s.StartsWith('[')) return JArray.Parse(s);
                if (s.StartsWith('{')) return JObject.Parse(s);
            }
            catch { }
            return null;
        }
    }

    return (Post, typeName);
}

var asAdmin = As(admin.UserMasterID, UserTypeCode.Admin, "Admin", admin.MobileNumber, admin.EmailID, admin.SessionEpoch).post;

Console.WriteLine($"API      : {baseUrl}");
Console.WriteLine($"Database : {db}");
Console.WriteLine();

// ── Outstanding report ───────────────────────────────────────────────────────────────────
Console.WriteLine("Outstanding report");
try
{
    var r = await asAdmin("CashCollection/OutstandingReport",
        new { UserID = 0, SearchText = "", TakeCount = 0, SkipCount = 0, IsExcelDownload = false });

    var rows = (r["data"] as JArray) ?? new JArray();
    Console.WriteLine($"  {rows.Count} account(s), total {r["TotalOutstanding"]}");

    decimal D(JToken? t) => t?.Value<decimal>() ?? 0m;

    // Each row's buckets total its balance. This is the one invariant the whole report rests on.
    bool everyRowTies = true;
    foreach (var row in rows)
    {
        decimal buckets = D(row["Age0To30"]) + D(row["Age31To60"]) + D(row["Age61To90"]) + D(row["Age90Plus"]);
        if (Math.Abs(buckets - D(row["Outstanding"])) > 0.005m)
        {
            everyRowTies = false;
            Console.WriteLine($"        {row["FirmName"]}: buckets {buckets} vs outstanding {row["Outstanding"]}");
        }
    }
    Check("every row's ageing buckets total its balance", everyRowTies);

    decimal bucketTotal = D(r["TotalAge0To30"]) + D(r["TotalAge31To60"]) + D(r["TotalAge61To90"]) + D(r["TotalAge90Plus"]);
    Check("report bucket totals equal the report total", Math.Abs(bucketTotal - D(r["TotalOutstanding"])) <= 0.005m,
          $"{bucketTotal} vs {r["TotalOutstanding"]}");

    Check("the report totals are the server's, over every row",
          Math.Abs(D(r["TotalOutstanding"]) - rows.Sum(x => D(x["Outstanding"]))) <= 0.005m);

    Check("accounts-with-balance counts only accounts actually owing",
          (r["AccountsWithBalance"]?.Value<int>() ?? -1) == rows.Count(x => D(x["Outstanding"]) > 0));

    Check("over-limit count agrees with the rows", (r["OverLimitCount"]?.Value<int>() ?? -1)
          == rows.Count(x => x["IsOverLimit"]?.Value<bool>() == true));

    // A row over its limit must also be flagged, and one inside it must not be.
    bool flagsRight = rows.All(x =>
    {
        bool expect = D(x["MaxOutstanding"]) > 0 && D(x["Outstanding"]) > D(x["MaxOutstanding"]);
        return (x["IsOverLimit"]?.Value<bool>() ?? false) == expect;
    });
    Check("IsOverLimit is set exactly when the balance passed the limit", flagsRight);

    Check("the report carries an as-on date", (r["AsOn"]?.ToString() ?? "").Length > 0);
}
catch (Exception ex) { Check("outstanding report", false, ex.Message); }

// Scope. The report is a DOWNLINE view, so the retailer tier is refused outright rather than
// served an empty list - they have nobody under them, and the figure they do have (what they
// owe upwards) is on their own Outstanding screen. Asserted here because the alternative, an
// empty 200, would put a menu row in front of them that always looks broken.
if (retailer != null)
{
    try
    {
        var asRetailer = As(retailer.UserMasterID, UserTypeCode.Retailer, "Retailer",
            retailer.MobileNumber, retailer.EmailID, retailer.SessionEpoch).post;
        await asRetailer("CashCollection/OutstandingReport",
            new { UserID = 0, SearchText = "", TakeCount = 0, SkipCount = 0, IsExcelDownload = false });
        Check($"a retailer ({retailer.FirmName}) is refused the downline report", false, "it answered instead");
    }
    catch (Exception ex)
    {
        Check($"a retailer ({retailer.FirmName}) is refused the downline report",
              ex.Message.Contains("UNAUTHORIZED", StringComparison.OrdinalIgnoreCase), ex.Message);
    }

    // ...and the menu does not offer it to them, so they never meet that refusal.
    try
    {
        var asRetailer = As(retailer.UserMasterID, UserTypeCode.Retailer, "Retailer",
            retailer.MobileNumber, retailer.EmailID, retailer.SessionEpoch).post;
        var m = await asRetailer("SidebarMenu/Select", new { });
        Check("…and their sidebar does not list the Outstanding Report",
              !Urls(m["data"] ?? new JArray()).Contains("/OutstandingReport"));
    }
    catch (Exception ex) { Check("retailer menu", false, ex.Message); }
}

// ── The menu ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("Menu");

static List<string> Urls(JToken menu)
{
    var urls = new List<string>();
    void Walk(JToken n)
    {
        string? u = n["url"]?.ToString() ?? n["Url"]?.ToString();
        if (!string.IsNullOrWhiteSpace(u)) urls.Add(u);
        foreach (var c in (n["children"] as JArray) ?? (n["Children"] as JArray) ?? new JArray()) Walk(c);
    }
    foreach (var n in (menu as JArray) ?? new JArray()) Walk(n);
    return urls;
}

async Task<List<string>> AdminMenu()
{
    var m = await asAdmin("SidebarMenu/Select", new { });
    return Urls(m["data"] ?? new JArray());
}

try
{
    var before = await AdminMenu();
    Check("the sidebar lists the new Outstanding Report", before.Contains("/OutstandingReport"));
    Check("the sidebar lists Menu Visibility", before.Contains("/MenuVisibility"));

    var catalog = await asAdmin("MenuVisibility/Select", new { UserTypeID = 0 });
    var items = (catalog["data"] as JArray) ?? new JArray();
    Check("the visibility catalogue covers the whole menu", items.Count > 50, $"{items.Count} entries");
    Check("every entry starts as shipped (no decisions stored)",
          items.All(i => i["HasOverride"]?.Value<bool>() != true));

    // Hide one report, and check the LIVE menu endpoint — not the admin read — stops listing it.
    const string victim = "/HoldReport";
    await asAdmin("MenuVisibility/Save", new
    {
        Items = new[] { new { MenuKey = victim, MenuName = "Balance Hold Report", UserTypeID = 0, IsVisible = false } },
    });

    var after = await AdminMenu();
    Check($"hiding {victim} removes it from the sidebar", !after.Contains(victim));
    Check("…and leaves every other entry alone", after.Count == before.Count - 1,
          $"{before.Count} → {after.Count}");

    // Restore it, and check it comes back — the cache between the write and the menu read is
    // exactly where a feature like this fails silently.
    await asAdmin("MenuVisibility/Save", new
    {
        Items = new[] { new { MenuKey = victim, MenuName = "Balance Hold Report", UserTypeID = 0, IsVisible = true } },
    });
    var restored = await AdminMenu();
    Check($"restoring {victim} brings it back immediately", restored.Contains(victim));
    Check("…and the menu is exactly what it was", restored.Count == before.Count);

    // A key the shipped definition does not have is refused rather than stored, or the table
    // fills with rows that hide nothing and cannot be told from a renamed screen.
    var junk = await asAdmin("MenuVisibility/Save", new
    {
        Items = new[] { new { MenuKey = "/NoSuchScreenAnywhere", MenuName = "Nope", UserTypeID = 0, IsVisible = false } },
    });
    Check("an unknown menu key is ignored", (junk["Changed"]?.Value<int>() ?? -1) == 0);
}
catch (Exception ex) { Check("menu visibility", false, ex.Message); }

// ── Month-end invoicing ──────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("Month-end invoicing");

const string Month = "202610";
try
{
    // The current month cannot be invoiced: it is still arriving.
    var thisMonth = $"{DateTime.Today:yyyyMM}";
    var now = await asAdmin("TaxInvoice/GenerateMonth", new { PeriodKey = thisMonth, DryRun = true });
    Check("the month in progress is refused", now["Refused"]?.Value<bool>() == true,
          now["RefusalReason"]?.ToString() ?? "");

    // The first-issue checks below need a month that has NOT been invoiced yet. Whether that is
    // true depends on the database this runs against — a copy taken before October was issued, or
    // one taken after — so it is detected rather than assumed. Asserting it blindly made four
    // checks fail for a reason that had nothing to do with the code.
    var existing = await asAdmin("TaxInvoice/Select", new
    {
        PeriodFrom = "2026-10-01", PeriodTo = "2026-10-31",
        Direction = "SALE", DocumentType = "", PartyUserID = 0,
    });
    bool alreadyInvoiced = ((existing as JArray) ?? new JArray())
        .Any(d => !string.Equals(d["Status"]?.ToString(), "Cancelled", StringComparison.OrdinalIgnoreCase));

    if (alreadyInvoiced)
    {
        Console.WriteLine("  ----  this month is already invoiced in this database; "
                        + "the first-issue checks need a fresh month and are skipped");
    }
    else
    {
        // With a party that has no place of supply, it must refuse and name them — and write nothing.
        var blocked = await asAdmin("TaxInvoice/GenerateMonth", new { PeriodKey = Month, DryRun = false });
        Check("a month with a stateless party is refused outright", blocked["Refused"]?.Value<bool>() == true,
              blocked["RefusalReason"]?.ToString() ?? "it issued instead");
        Check("…and the refusal names who to fix", ((blocked["Blockers"] as JArray)?.Count ?? 0) > 0);
        Check("…and nothing was issued", blocked["Issued"]?.Value<bool>() != true);

        // Give that party a state, and the same call should go through.
        await asAdmin("PartyState/SetPlaceOfSupply", new { UserID = 2, PlaceOfSupplyStateCode = "27", CreatedByUserID = 0 });

        var dry = await asAdmin("TaxInvoice/GenerateMonth", new { PeriodKey = Month, DryRun = true });
        int wouldIssue = dry["DocumentsGenerated"]?.Value<int>() ?? 0;
        Check("once the state is set, the preview is clean", dry["Refused"]?.Value<bool>() != true,
              dry["RefusalReason"]?.ToString() ?? "");
        Check("…and the preview wrote nothing", dry["Issued"]?.Value<bool>() != true);
        Check("…and it has documents to issue", wouldIssue > 0, $"{wouldIssue}");

        var issued = await asAdmin("TaxInvoice/GenerateMonth", new { PeriodKey = Month, DryRun = false });
        int count = issued["DocumentsGenerated"]?.Value<int>() ?? 0;
        Check("issuing produces the documents the preview promised",
              issued["Issued"]?.Value<bool>() == true && count == wouldIssue,
              $"preview {wouldIssue}, issued {count}");
    }

    // The one that makes a schedule safe: a second run must issue NOTHING.
    var again = await asAdmin("TaxInvoice/GenerateMonth", new { PeriodKey = Month, DryRun = false });
    Check("running the same month again issues nothing",
          (again["DocumentsGenerated"]?.Value<int>() ?? -1) == 0,
          $"{again["DocumentsGenerated"]} document(s) the second time");
    Check("…and says the sources were already invoiced",
          (again["SkippedAlreadyInvoiced"]?.Value<int>() ?? 0) > 0);

    // ── Regrouping a month already invoiced ──────────────────────────────────────────────
    //
    // The case the business actually has: October went out one invoice per DAY before anybody
    // was asked, and what was wanted is one per customer per month. That is a cancel-and-reissue,
    // and the things worth asserting are that it withdraws exactly what was there, that the new
    // documents are FEWER (which is the whole point), and that the money is unchanged — a
    // regrouping that moved a rupee would be a restatement, not a regrouping.
    var before = await asAdmin("TaxInvoice/Select", new
    {
        PeriodFrom = "2026-10-01", PeriodTo = "2026-10-31",
        Direction = "SALE", DocumentType = "", PartyUserID = 0,
    });
    var beforeRows = (before as JArray) ?? new JArray();
    decimal beforeValue = beforeRows.Sum(d => d["GrandTotal"]?.Value<decimal>() ?? 0);
    int beforeCount = beforeRows.Count;

    var dryRegroup = await asAdmin("TaxInvoice/RegroupMonth", new
    {
        PeriodKey = Month, DryRun = true, Consolidation = "MONTHLY", SaveAsDefault = false,
    });
    Check("a regroup dry run writes nothing", dryRegroup["Issued"]?.Value<bool>() != true);
    Check("…and says how many documents it would withdraw",
          ((dryRegroup["Warnings"] as JArray) ?? new JArray())
              .Any(w => w.ToString().Contains("would be CANCELLED", StringComparison.OrdinalIgnoreCase)));

    var regrouped = await asAdmin("TaxInvoice/RegroupMonth", new
    {
        PeriodKey = Month, DryRun = false, Consolidation = "MONTHLY", SaveAsDefault = true,
    });
    Check("the regroup cancelled the documents that were there",
          (regrouped["CancelledDocuments"]?.Value<int>() ?? 0) == beforeCount,
          $"cancelled {regrouped["CancelledDocuments"]}, there were {beforeCount}");

    var after = await asAdmin("TaxInvoice/Select", new
    {
        PeriodFrom = "2026-10-01", PeriodTo = "2026-10-31",
        Direction = "SALE", DocumentType = "", PartyUserID = 0,
    });
    var live = ((after as JArray) ?? new JArray())
        .Where(d => !string.Equals(d["Status"]?.ToString(), "Cancelled", StringComparison.OrdinalIgnoreCase))
        .ToList();
    decimal afterValue = live.Sum(d => d["GrandTotal"]?.Value<decimal>() ?? 0);

    Check("monthly grouping produces fewer documents than daily did",
          live.Count > 0 && live.Count < beforeCount, $"{beforeCount} → {live.Count}");
    Check("…and not one rupee moved", Math.Abs(afterValue - beforeValue) <= 0.01m,
          $"{beforeValue:N2} → {afterValue:N2}");
    // At most two: a customer can legitimately receive one invoice for their wallet top-ups and
    // one for the commission and charges on their transactions. Those are different supplies with
    // different SACs and they cannot share a line, so "one invoice per customer" would be the
    // wrong rule to assert — the right one is that neither KIND is issued more than once.
    Check("…and no customer gets more than one invoice per kind of supply",
          live.GroupBy(d => d["PartyUserID"]?.Value<int>() ?? 0).All(g => g.Count() <= 2),
          string.Join(", ", live.GroupBy(d => d["PartyName"]?.ToString())
                                .Select(g => $"{g.Key}:{g.Count()}")));

    // The cancelled numbers are kept — GSTR-1 table 13 has to declare them.
    int cancelled = ((after as JArray) ?? new JArray())
        .Count(d => string.Equals(d["Status"]?.ToString(), "Cancelled", StringComparison.OrdinalIgnoreCase));
    Check("the withdrawn numbers are kept as cancelled, not deleted", cancelled == beforeCount,
          $"{cancelled} cancelled");

    // The return must still balance after all that.
    var g1 = await asAdmin("GstReturn/Gstr1", new { CompanyGSTIN = "27ABCCS3656G1Z8", PeriodKey = Month, B2CLThreshold = 0m });
    var g3 = await asAdmin("GstReturn/Gstr3b", new { CompanyGSTIN = "27ABCCS3656G1Z8", PeriodKey = Month, B2CLThreshold = 0m });
    Check("GSTR-1 still agrees with 3B after the regroup", g3["AgreesWithGstr1"]?.Value<bool>() == true,
          $"variance tax {g3["VarianceTax"]}");
    Check("…and GSTR-1's taxable value is unchanged by the regrouping",
          Math.Abs((g1["TotalTaxableValue"]?.Value<decimal>() ?? 0) - 73923.85m) <= 0.01m,
          $"{g1["TotalTaxableValue"]}");

}
catch (Exception ex) { Check("month-end invoicing", false, ex.Message); }

Console.WriteLine();
Console.WriteLine($"{passed} passed, {failed} failed");
return failed == 0 ? 0 : 1;
