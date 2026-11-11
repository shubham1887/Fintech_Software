using System.Net.Http.Headers;
using System.Text;
using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

// Drives the RUNNING API over HTTP as a real signed-in user, with a token minted from the same
// settings the app reads — so the answer comes through the whole stack: the JWT filter, the
// deny-by-default access policy, the hierarchy scope middleware, the controller and the service.
//
// Every endpoint here is one the distribution tier was locked out of until this change, which is
// the thing worth checking end to end rather than unit by unit.
//
//   dotnet run --project tools/probe-api -- http://127.0.0.1:5099 FinoTest_Delegation

string baseUrl = (args.ElementAtOrDefault(0) ?? "http://127.0.0.1:5099").TrimEnd('/');
string db = args.ElementAtOrDefault(1) ?? string.Empty;
if (!db.StartsWith("FinoTest", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Refusing to run: pass a throwaway database whose name starts with 'FinoTest'.");
    return 2;
}

// The same configuration the running instance loaded, so the token it is handed verifies and the
// envelope it is sent decrypts.
string settingsPath = Path.Combine(@"C:\Temp\finotest\publish-api", "appsettings.json");
var config = new ConfigurationBuilder().AddJsonFile(settingsPath, optional: false).Build();
var jwt = new JwtTokenService(config);
var crypto = new EncryptDecrypt(config["Encryption:Key"] ?? "", config["Encryption:IV"] ?? "");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer($"Server=localhost\\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;")
    .Options;
await using var ctx = new ApplicationDbContext(options);

var distributor = await ctx.UserMasters.AsNoTracking()
    .FirstOrDefaultAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Distributor);
if (distributor == null) { Console.WriteLine("No distributor in this database."); return 3; }

var retailer = await ctx.UserMasters.AsNoTracking()
    .FirstOrDefaultAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Retailer
                              && u.ParentID == distributor.UserMasterID);

Console.WriteLine($"API          : {baseUrl}");
Console.WriteLine($"Signed in as : {distributor.FirmName} (#{distributor.UserMasterID}), Distributor");
Console.WriteLine();

var tokens = jwt.GenerateJwtToken(distributor.UserMasterID, UserTypeCode.Distributor, "Distributor",
    distributor.MobileNumber, distributor.EmailID, distributor.SessionEpoch);

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

string keyData = crypto.Encrypt(JsonConvert.SerializeObject(new
{
    UserID = distributor.UserMasterID,
    UserTypeID = UserTypeCode.Distributor,
    SessionToken = tokens.AccessToken,
    DeviceType = "Web",
    IPAddress = "127.0.0.1",
}));

int passed = 0, failed = 0;
void Check(string what, bool ok, string detail = "")
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail.Length > 0 ? $"  — {detail}" : "")}");
    if (ok) passed++; else failed++;
}

async Task<(int status, bool ok, string message, string data)> Post(string path, object? body = null)
{
    var form = new MultipartFormDataContent
    {
        { new StringContent(keyData), "KeyData" },
        { new StringContent(crypto.Encrypt(JsonConvert.SerializeObject(body ?? new { }))), "Request" },
    };
    var res = await http.PostAsync($"{baseUrl}/api/{path}", form);
    string raw = await res.Content.ReadAsStringAsync();
    if (!res.IsSuccessStatusCode) return ((int)res.StatusCode, false, raw, string.Empty);

    dynamic? env = JsonConvert.DeserializeObject<dynamic>(raw);
    string status = (string?)env?.status ?? (string?)env?.Status ?? string.Empty;
    string message = (string?)env?.message ?? (string?)env?.Message ?? string.Empty;
    string payload = (string?)env?.data ?? (string?)env?.Data ?? string.Empty;
    string plain = string.Empty;
    if (!string.IsNullOrEmpty(payload)) { try { plain = crypto.Decrypt(payload); } catch { plain = payload; } }
    return ((int)res.StatusCode, status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase), message, plain);
}

const string NoPermission = "You don't have permission to perform this action.";

// ── The Add User flow, as a distributor actually walks it ────────────────────────────────────
Console.WriteLine("Add User (the screens a distributor opens, in order)");

var probes = new (string Path, object Body)[]
{
    ("UserType/GetDropdown", new { }),
    ("State/GetDropdown", new { }),
    ("UserCode/Preview", new { UserTypeID = UserTypeCode.Retailer }),
    ("Users/SelectForDDLWithUserType", new { UserTypeID = UserTypeCode.Retailer }),
    ("Slabs/GetDropdown", new { }),
    ("VerificationDocument/GetAll", new { }),
    ("Service/GetAll", new { IsActive = true, PageNumber = 1, PageSize = 500 }),
    ("Users/CheckAvailability", new { MobileNumber = "9000000001" }),
};
foreach (var (path, body) in probes)
{
    var r = await Post(path, body);
    // A business FAIL ("No record found") is a fine answer here; a PERMISSION refusal is not.
    Check(path, r.status == 200 && !r.message.Contains(NoPermission), $"{r.status} {r.message}");
}

// The submit itself. A mobile number that is certainly free, so a success is a real creation.
// A 10-digit number that is certainly free: 9 + a millisecond-ish stamp.
string mobile = "9" + DateTime.UtcNow.ToString("HHmmssfff");
var insert = await Post("Users/Insert", new
{
    FirmName = "ZZ Probe Retailer",
    PersonName = "ZZ Probe",
    MobileNumber = mobile,
    UserTypeID = UserTypeCode.Retailer,
    ParentID = distributor.UserMasterID,
});
Check("Users/Insert is reachable (no permission refusal)",
    !insert.message.Contains(NoPermission), $"{insert.status} {insert.message}");
Check("Users/Insert actually creates the user", insert.ok, insert.message);

// And the two things a distributor must still NOT be able to do.
var activate = await Post("Users/Activate", new { TargetUserID = retailer?.UserMasterID ?? 1, IsActive = true });
Check("Users/Activate is still refused", activate.message.Contains(NoPermission)
    || activate.message.Contains("Only an administrator"), $"{activate.status} {activate.message}");
var del = await Post("Users/Delete", new { TargetUserID = retailer?.UserMasterID ?? 1 });
Check("Users/Delete is still refused", del.message.Contains(NoPermission), $"{del.status} {del.message}");

// ── Delegation, over HTTP ───────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("Commission slab delegation");

var parent = await Post("SlabDelegation/MyParentSlab");
Check("MyParentSlab answers", parent.status == 200 && !parent.message.Contains(NoPermission),
    $"{parent.status} {parent.message}");

var mine = await Post("SlabDelegation/MySlabs");
Check("MySlabs answers", mine.ok, mine.message);
Check("and lists the card cut during the service-level run",
    mine.data.Contains("ZZ Verify Retailer", StringComparison.OrdinalIgnoreCase),
    mine.data.Length > 160 ? mine.data[..160] + "…" : mine.data);

var assignments = await Post("SlabDelegation/Assignments");
Check("Assignments answers", assignments.status == 200 && !assignments.message.Contains(NoPermission),
    $"{assignments.status} {assignments.message}");

var audit = await Post("SlabDelegation/Audit", new { PageNumber = 1, PageSize = 20 });
Check("Audit answers", audit.ok, audit.message);

// Admin's own slab editor must stay shut to a distributor.
var adminSlab = await Post("Slabs/Create", new { SlabName = "ZZ Should Not Exist", IsActive = true });
Check("Slabs/Create is still Admin-only", adminSlab.message.Contains(NoPermission),
    $"{adminSlab.status} {adminSlab.message}");
var adminRow = await Post("SlabDetail/Update", new { SlabDetailID = 1, SlabMasterID = 1 });
Check("SlabDetail/Update is still Admin-only", adminRow.message.Contains(NoPermission),
    $"{adminRow.status} {adminRow.message}");

// ── The reports a distributor reads ─────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("Reports");

var today = DateTime.Now.ToString("yyyy-MM-dd");
var reportProbes = new (string Path, string[] ExpectKeys)[]
{
    ("TransactionReport/SelectForRechargeFilter", new[] { "Success Amount", "Failed Amount", "Pending Amount", "Commission" }),
    ("TransactionReport/SelectForDMTFilter", new[] { "Success Amount", "Commission" }),
    ("TransactionReport/SelectForBBPS", new[] { "Success Amount", "Commission" }),
};
foreach (var (path, keys) in reportProbes)
{
    var r = await Post(path, new
    {
        FromDate = "2026-01-01", ToDate = today, Status = "ALL", TakeCount = 5, SkipCount = 0, UserID = 0,
    });
    Check(path, r.status == 200 && !r.message.Contains(NoPermission), $"{r.status} {r.message}");
    foreach (var k in keys)
        Check($"   strip carries \"{k}\"", r.data.Contains(k, StringComparison.Ordinal),
            r.data.Length == 0 ? "(empty payload)" : "");
}

var stmt = await Post("TransactionDetails/SelectForFilter", new
{
    FromDate = "2026-01-01", ToDate = today, TakeCount = 5, SkipCount = 0,
});
Check("TransactionDetails/SelectForFilter", stmt.status == 200 && !stmt.message.Contains(NoPermission),
    $"{stmt.status} {stmt.message}");
foreach (var k in new[] { "Opening Balance", "Closing Balance", "Difference", "Charge" })
    Check($"   statement strip carries \"{k}\"", stmt.data.Contains(k, StringComparison.Ordinal));
Check("   a distributor's statement names no provider / counterparty",
    !stmt.data.Contains("\"Counterparty\"", StringComparison.Ordinal)
        || !System.Text.RegularExpressions.Regex.IsMatch(stmt.data, "\"Counterparty\"\s*:\s*\"[^\"]+\""),
    "the field must come back blank for a downline role, not merely be hidden in the browser");

var fund = await Post("Fund/TransferReport", new
{
    FromDate = "2026-01-01", ToDate = today, Status = "ALL", TakeCount = 5, SkipCount = 0,
});
Check("Fund/TransferReport", fund.status == 200 && !fund.message.Contains(NoPermission),
    $"{fund.status} {fund.message}");
Check("   fund strip carries \"Success Amount\"", fund.data.Contains("Success Amount", StringComparison.Ordinal),
    fund.data.Length == 0 ? "(empty payload)" : "");

var settlement = await Post("Fund/SelectWalletTransferReport", new
{
    FromDate = "2026-01-01", ToDate = today, TakeCount = 5, SkipCount = 0,
});
Check("Fund/SelectWalletTransferReport (Settlement Report)",
    settlement.status == 200 && !settlement.message.Contains(NoPermission),
    $"{settlement.status} {settlement.message}");

var logins = await Post("LoginHistory/GetAll", new { PageNumber = 1, PageSize = 10 });
Check("LoginHistory/GetAll", logins.status == 200 && !logins.message.Contains(NoPermission),
    $"{logins.status} {logins.message}");
Check("   rows carry the user's name, resolved server-side",
    logins.data.Contains("UserName", StringComparison.Ordinal) || logins.data.Length == 0,
    logins.data.Length == 0 ? "(no login rows yet)" : "");
Check("   and no longer carry the session Token",
    !logins.data.Contains("\"Token\"", StringComparison.Ordinal));

Console.WriteLine();
Console.WriteLine($"{passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
