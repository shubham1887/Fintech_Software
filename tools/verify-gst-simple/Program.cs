using System.Net.Http.Headers;
using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Drives the RUNNING API as a signed-in admin and as an accountant, through the whole stack,
// over the simplified GST screens: the purchase invoice (lookups, add, view, edit, delete, the
// item list), the invoice generator's preview, the month summary and the "upload 2B, the rest
// follows" act — and the menu each role is shown.
//
//   dotnet run --project tools/verify-gst-simple -- http://127.0.0.1:5266 finocore_gstui_test [session]
//
// It WRITES (a purchase bill, a 2B import, credit claims), so it refuses any database whose name
// does not contain "test". Passing "session" as the third argument only prints a browser
// session for the admin, for looking at the screens by hand.

string baseUrl = (args.ElementAtOrDefault(0) ?? "http://127.0.0.1:5266").TrimEnd('/');
string db = args.ElementAtOrDefault(1) ?? string.Empty;
bool sessionOnly = string.Equals(args.ElementAtOrDefault(2), "session", StringComparison.OrdinalIgnoreCase);
if (db.IndexOf("test", StringComparison.OrdinalIgnoreCase) < 0)
{
    Console.WriteLine("Refusing to run: pass a throwaway database whose name contains 'test'.");
    return 2;
}

string apiDir = @"C:\Users\Administrator\Desktop\Fintech_Software\Fino_API";
var config = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(apiDir, "appsettings.json"), optional: false)
    .AddJsonFile(Path.Combine(apiDir, "appsettings.Development.json"), optional: true)
    .Build();
var jwt = new JwtTokenService(config);
var crypto = new EncryptDecrypt(config["Encryption:Key"] ?? "", config["Encryption:IV"] ?? "");

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer($"Server=localhost\\SQLEXPRESS;Database={db};Trusted_Connection=True;TrustServerCertificate=True;")
    .Options;
await using var ctx = new ApplicationDbContext(options);

var admin = await ctx.UserMasters.AsNoTracking().FirstAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Admin);
var accountant = await ctx.UserMasters.AsNoTracking().FirstOrDefaultAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Accountant);

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };

(string token, string keyData) SignIn(FintechApp.Entities.UserMaster u, string role)
{
    int typeId = Convert.ToInt32(u.UserTypeID);
    var t = jwt.GenerateJwtToken(u.UserMasterID, typeId, role, u.MobileNumber, u.EmailID, u.SessionEpoch);
    string kd = crypto.Encrypt(JsonConvert.SerializeObject(new
    {
        UserID = u.UserMasterID, UserTypeID = typeId, SessionToken = t.AccessToken,
        DeviceType = "Web", IPAddress = "127.0.0.1",
    }));
    return (t.AccessToken, kd);
}

var adminAuth = SignIn(admin, "Admin");

if (sessionOnly)
{
    Console.WriteLine(JsonConvert.SerializeObject(new
    {
        admin.UserMasterID, admin.UserTypeID, admin.FirmName, admin.PersonName, admin.MobileNumber, admin.EmailID,
        Token = adminAuth.token,
    }));
    return 0;
}

int passed = 0, failed = 0;
void Check(string what, bool ok, string detail = "")
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail.Length > 0 ? $"  — {detail}" : "")}");
    if (ok) passed++; else failed++;
}

async Task<(int status, bool ok, string message, JToken data)> Post((string token, string keyData) who, string path, object body = null)
{
    var form = new MultipartFormDataContent
    {
        { new StringContent(who.keyData), "KeyData" },
        { new StringContent(crypto.Encrypt(JsonConvert.SerializeObject(body ?? new { }))), "Request" },
    };
    var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/{path}") { Content = form };
    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", who.token);
    var res = await http.SendAsync(req);
    string raw = await res.Content.ReadAsStringAsync();
    if (!res.IsSuccessStatusCode) return ((int)res.StatusCode, false, raw.Length > 200 ? raw[..200] : raw, null);

    var env = JObject.Parse(raw);
    string status = (string)(env["status"] ?? env["Status"]) ?? string.Empty;
    string message = (string)(env["message"] ?? env["Message"]) ?? string.Empty;
    string payload = (string)(env["data"] ?? env["Data"]) ?? string.Empty;
    JToken data = null;
    if (!string.IsNullOrEmpty(payload))
    {
        string plain;
        try { plain = crypto.Decrypt(payload); } catch { plain = payload; }
        try { data = JToken.Parse(plain); } catch { data = new JValue(plain); }
    }
    return ((int)res.StatusCode, status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase), message, data);
}

decimal D(JToken t, string name) => (decimal?)t?[name] ?? 0m;

Console.WriteLine($"API {baseUrl}  ·  database {db}  ·  admin #{admin.UserMasterID}");
Console.WriteLine();

// ── Purchase invoice ────────────────────────────────────────────────────────────────────────
Console.WriteLine("Purchase Invoice");
var lk = await Post(adminAuth, "Purchase/Lookups");
Check("Lookups load", lk.ok, lk.message);
var providers = lk.data?["Providers"] as JArray ?? new JArray();
var wallets = lk.data?["Wallets"] as JArray ?? new JArray();
var items = lk.data?["Items"] as JArray ?? new JArray();
var suppliers = lk.data?["Suppliers"] as JArray ?? new JArray();
Check("Providers, wallets and items are lists to choose from",
    providers.Count > 0 && wallets.Count > 0 && items.Count >= 5,
    $"{providers.Count} providers, {wallets.Count} wallets, {items.Count} items, {suppliers.Count} suppliers seen");
Check("Company state is known (decides CGST+SGST vs IGST)", ((string)lk.data?["CompanyStateCode"] ?? "").Length == 2,
    (string)lk.data?["CompanyStateCode"]);
var jaijui = providers.FirstOrDefault(p => ((string)p["ProviderName"] ?? "").StartsWith("JaiJui", StringComparison.OrdinalIgnoreCase));
Check("A provider with no bill yet is offered the matching known supplier",
    jaijui != null && ((string)jaijui["SupplierGSTIN"] ?? "").Length == 15,
    jaijui == null ? "no JaiJui provider" : $"{jaijui["ProviderName"]} → {jaijui["SupplierName"]} ({jaijui["SupplierGSTIN"]}), guessed={jaijui["Guessed"]}");

var list0 = await Post(adminAuth, "Purchase/Select", new { PeriodFrom = "2026-04-01", PeriodTo = "2027-03-31" });
int before = (list0.data as JArray)?.Count ?? 0;
Check("The list loads with each bill's credit status", list0.ok && before > 0 && list0.data[0]["ItcStatus"] != null, $"{before} bills");

// A bill entered the way a supplier writes it: quantity × rate, GST worked out, intra-state.
int providerId = (int)(jaijui?["ProviderID"] ?? providers[0]["ProviderID"]);
int walletId = (int)wallets[0]["WalletID"];
string docNo = "ZZTEST/" + DateTime.UtcNow.ToString("HHmmss");
var capture = await Post(adminAuth, "Purchase/Capture", new
{
    SupplierName = "JAIJUI FINTECH PRIVATE LIMITED", SupplierGSTIN = "27AAFCJ8090F1ZC", SupplierStateCode = "27",
    DocumentNo = docNo, DocumentDate = "2026-11-05", ProviderID = providerId, WalletID = walletId, RoundOff = 0m,
    Remark = "verify-gst-simple",
    Lines = new[]
    {
        new { Description = "Domestic Money Transfer", SACCode = "997112", Quantity = 1, TaxableValue = 8300.00m,
              CGSTRate = 9m, CGSTAmount = 747.00m, SGSTRate = 9m, SGSTAmount = 747.00m, IGSTRate = 0m, IGSTAmount = 0m,
              CessAmount = 0m, ITCEligible = true, ReverseCharge = false,
              UnitQuantity = 1000000m, UnitRate = 0.83m, RateType = "PERCENT" },
    },
});
int newId = (int?)capture.data?["TaxInvoiceHeaderID"] ?? 0;
Check("A bill is saved", capture.ok && newId > 0, $"{capture.message} id {newId}, total {D(capture.data, "GrandTotal")}, credit {D(capture.data, "EligibleITC")}");
Check("Its credit is in the register at once (no separate Build step)", D(capture.data, "EligibleITC") == 1494.00m);

var again = await Post(adminAuth, "Purchase/Capture", new
{
    SupplierName = "JAIJUI FINTECH PRIVATE LIMITED", SupplierGSTIN = "27AAFCJ8090F1ZC", DocumentNo = docNo, DocumentDate = "2026-11-05",
    Lines = new[] { new { Description = "x", TaxableValue = 1m } },
});
Check("Entering the same bill twice adds nothing", again.ok && (bool?)again.data?["AlreadyExisted"] == true);

var get = await Post(adminAuth, "Purchase/GetById", new { TaxInvoiceHeaderID = newId });
var line0 = (get.data?["Lines"] as JArray)?.FirstOrDefault();
Check("View shows quantity, rate, provider and wallet",
    get.ok && D(line0, "UnitQuantity") == 1000000m && D(line0, "UnitRate") == 0.83m
    && ((string)get.data["ProviderName"] ?? "").Length > 0 && ((string)get.data["WalletName"] ?? "").Length > 0,
    $"{line0?["UnitQuantity"]} × {line0?["UnitRate"]} {line0?["RateType"]}, {get.data?["ProviderName"]}, {get.data?["WalletName"]}");

var lk2 = await Post(adminAuth, "Purchase/Lookups");
var learned = (lk2.data?["Providers"] as JArray)?.FirstOrDefault(p => (int)p["ProviderID"] == providerId);
Check("The provider now fills its supplier from that bill, not a guess",
    learned != null && (string)learned["SupplierGSTIN"] == "27AAFCJ8090F1ZC" && (bool)learned["Guessed"] == false && (int)learned["WalletID"] == walletId);

var update = await Post(adminAuth, "Purchase/Update", new
{
    TaxInvoiceHeaderID = newId,
    SupplierName = "JAIJUI FINTECH PRIVATE LIMITED", SupplierGSTIN = "27AAFCJ8090F1ZC", SupplierStateCode = "27",
    DocumentNo = docNo, DocumentDate = "2026-11-05", ProviderID = providerId, WalletID = walletId, RoundOff = 0.01m,
    Remark = "corrected",
    Lines = new[]
    {
        new { Description = "Domestic Money Transfer", SACCode = "997112", Quantity = 1, TaxableValue = 8300.00m,
              CGSTRate = 9m, CGSTAmount = 747.00m, SGSTRate = 9m, SGSTAmount = 747.00m, IGSTRate = 0m, IGSTAmount = 0m,
              CessAmount = 0m, ITCEligible = true, ReverseCharge = false, UnitQuantity = 1000000m, UnitRate = 0.83m, RateType = "PERCENT" },
        new { Description = "Verification", SACCode = "997159", Quantity = 1, TaxableValue = 1000.00m,
              CGSTRate = 9m, CGSTAmount = 90.00m, SGSTRate = 9m, SGSTAmount = 90.00m, IGSTRate = 0m, IGSTAmount = 0m,
              CessAmount = 0m, ITCEligible = true, ReverseCharge = false, UnitQuantity = 2000m, UnitRate = 0.5m, RateType = "UNIT" },
    },
});
Check("Edit: a second line and a round-off are saved", update.ok && D(update.data, "GrandTotal") == 10974.01m && D(update.data, "EligibleITC") == 1674.00m,
    $"{update.message} total {D(update.data, "GrandTotal")}, credit {D(update.data, "EligibleITC")}");

var get2 = await Post(adminAuth, "Purchase/GetById", new { TaxInvoiceHeaderID = newId });
Check("Edit: exactly the new lines are live", (get2.data?["Lines"] as JArray)?.Count == 2);

// ── Items ───────────────────────────────────────────────────────────────────────────────────
var item = await Post(adminAuth, "Purchase/SaveItem", new { Name = "ZZ Test Item", SACCode = "998599", GSTRatePercent = 18m, DefaultRate = 0.001m, RateType = "UNIT", ITCEligible = true, IsActive = true });
int itemId = (int?)item.data?["PurchaseItemMasterID"] ?? 0;
Check("An item with its own rate can be added", item.ok && itemId > 0, item.message);
var dup = await Post(adminAuth, "Purchase/SaveItem", new { Name = "ZZ Test Item", GSTRatePercent = 18m });
Check("A duplicate item name is refused in words", !dup.ok && dup.message.Contains("already exists"), dup.message);
var delItem = await Post(adminAuth, "Purchase/DeleteItem", new { PurchaseItemMasterID = itemId });
Check("An item can be removed", delItem.ok, delItem.message);

// ── Invoice generator preview ───────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("Generate sale invoices");
string thisMonth = DateTime.Today.ToString("yyyyMM");
var prev = await Post(adminAuth, "TaxInvoice/GenerateMonth", new { PeriodKey = thisMonth, DryRun = true, Consolidation = "MONTHLY", SaveAsDefault = false });
var docs = prev.data?["Documents"] as JArray ?? new JArray();
Check("This month (so far) previews instead of being refused", prev.ok && (bool?)prev.data?["Refused"] != true,
    $"{prev.message}{prev.data?["RefusalReason"]} · {docs.Count} invoices, partial={prev.data?["PartialMonth"]}");
if (docs.Count > 0)
{
    var d0 = docs[0];
    Check("Each previewed invoice says who, what, how many, the rate and the amount",
        ((string)d0["PartyName"] ?? "").Length > 0 && ((string)d0["Description"] ?? "").Length > 0
        && (int)d0["SourceCount"] > 0 && D(d0, "GrandTotal") > 0 && ((string)d0["ServiceName"] ?? "").Length > 0,
        $"{d0["PartyName"]} · {d0["ServiceName"]} · {d0["SourceCount"]} · {d0["GSTRatePercent"]}% · {D(d0, "GrandTotal")}");
    decimal sumDocs = docs.Sum(x => D(x, "TaxableTotal"));
    Check("The rows add up to the totals", Math.Abs(sumDocs - D(prev.data, "TaxableTotal")) < 0.01m, $"{sumDocs} vs {D(prev.data, "TaxableTotal")}");
}
string lastMonth = DateTime.Today.AddMonths(-1).ToString("yyyyMM");
var prevLast = await Post(adminAuth, "TaxInvoice/GenerateMonth", new { PeriodKey = lastMonth, DryRun = true, Consolidation = "MONTHLY", SaveAsDefault = false });
Check("Last month: already invoiced, nothing new", prevLast.ok && (int?)prevLast.data?["DocumentsGenerated"] == 0 && (int?)prevLast.data?["SkippedAlreadyInvoiced"] > 0,
    $"{prevLast.data?["DocumentsGenerated"]} new, {prevLast.data?["SkippedAlreadyInvoiced"]} already invoiced");

// ── The month ───────────────────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine($"GST Returns — {lastMonth}");
var m1 = await Post(adminAuth, "GstReturn/MonthSummary", new { PeriodKey = lastMonth });
Check("Month summary loads", m1.ok, m1.message);
Console.WriteLine($"        sales: {m1.data?["SaleInvoiceCount"]} invoices, GST {D(m1.data, "OutputTax")} (I {D(m1.data, "OutputIGST")} C {D(m1.data, "OutputCGST")} S {D(m1.data, "OutputSGST")})");
Console.WriteLine($"        purchases: {m1.data?["PurchaseCount"]} bills, GST {D(m1.data, "PurchaseTax")}; confirmed {D(m1.data, "CreditConfirmed")}, waiting {D(m1.data, "CreditWaiting")}");
Console.WriteLine($"        to pay {D(m1.data, "TotalPayable")}; if confirmed {D(m1.data, "PayableIfCreditConfirmed")}, carry {D(m1.data, "ClosingCreditIfConfirmed")}");
decimal output = D(m1.data, "OutputTax"), waiting = D(m1.data, "CreditWaiting");
Check("Before 2B: no credit is used, the full GST shows as payable", D(m1.data, "CreditConfirmed") == 0m && D(m1.data, "TotalPayable") == output && output > 0m);
Check("…and it says what happens once the credit is confirmed",
    D(m1.data, "PayableIfCreditConfirmed") == 0m && D(m1.data, "ClosingCreditIfConfirmed") == waiting - output,
    $"payable {D(m1.data, "PayableIfCreditConfirmed")}, carry {D(m1.data, "ClosingCreditIfConfirmed")} (= {waiting} − {output})");

// A GSTR-2B file as the portal writes it, built from the bills in the books — two of the three,
// so one is left as "supplier has not filed", plus one bill the books have never seen.
var bills = await ctx.TaxInvoiceHeaders.AsNoTracking()
    .Where(h => !h.IsDeleted && h.Direction == "PURCHASE" && h.PeriodKey == lastMonth).OrderBy(h => h.TaxInvoiceHeaderID).ToListAsync();
string companyGstin = (string)m1.data?["CompanyGSTIN"] ?? "";
JObject Inv(string no, DateTime dt, decimal val, decimal txval, decimal igst, decimal cgst, decimal sgst) => new()
{
    ["inum"] = no, ["typ"] = "R", ["dt"] = dt.ToString("dd-MM-yyyy"), ["val"] = val, ["pos"] = companyGstin[..2], ["rev"] = "N", ["itcavl"] = "Y",
    ["items"] = new JArray(new JObject { ["num"] = 1, ["rt"] = 18, ["txval"] = txval, ["igst"] = igst, ["cgst"] = cgst, ["sgst"] = sgst, ["cess"] = 0 }),
};
var b2b = new JArray();
foreach (var b in bills.Take(2))
    b2b.Add(new JObject
    {
        ["ctin"] = b.PartyGSTIN, ["trdnm"] = b.PartyName,
        ["inv"] = new JArray(Inv(b.DocumentNo, b.DocumentDate, b.GrandTotal, b.TaxableTotal, b.IGSTTotal, b.CGSTTotal, b.SGSTTotal)),
    });
b2b.Add(new JObject
{
    ["ctin"] = "29AAACZ9999Z1Z5", ["trdnm"] = "UNSEEN SUPPLIER LTD",
    ["inv"] = new JArray(Inv("UNSEEN/001", new DateTime(2026, 10, 12), 1180m, 1000m, 180m, 0m, 0m)),
});
string rtnprd = lastMonth.Substring(4, 2) + lastMonth.Substring(0, 4);
string twoB = new JObject { ["data"] = new JObject { ["gstin"] = companyGstin, ["rtnprd"] = rtnprd, ["gendt"] = "14-11-2026", ["docdata"] = new JObject { ["b2b"] = b2b } } }.ToString();

var wrongMonth = await Post(adminAuth, "GstReturn/Import2b", new { Json = twoB, PeriodKey = thisMonth });
Check("A 2B file for another month is refused, in words", !wrongMonth.ok && wrongMonth.message.Contains("month"), wrongMonth.message);
var wrongCo = await Post(adminAuth, "GstReturn/Import2b", new { Json = twoB.Replace(companyGstin, "07AAAAA0000A1Z5"), PeriodKey = lastMonth });
Check("A 2B file for another GSTIN is refused, in words", !wrongCo.ok && wrongCo.message.Contains("GSTIN"), wrongCo.message);
var junk = await Post(adminAuth, "GstReturn/Import2b", new { Json = "not json", PeriodKey = lastMonth });
Check("A file that is not a 2B is refused, in words", !junk.ok && junk.message.Contains("could not be read"), junk.message);

var imp = await Post(adminAuth, "GstReturn/Import2b", new { Json = twoB, PeriodKey = lastMonth });
Check("Uploading the 2B file matches and claims in one act",
    imp.ok && (int?)imp.data?["Matched"] == 2 && (int?)imp.data?["CreditsClaimed"] == 2 && (int?)imp.data?["MissingIn2B"] == 1 && (int?)imp.data?["MissingInBooks"] == 1,
    $"{imp.message} matched {imp.data?["Matched"]}, claimed {imp.data?["CreditsClaimed"]} ({D(imp.data, "CreditClaimedValue")}), not filed {imp.data?["MissingIn2B"]}, not in books {imp.data?["MissingInBooks"]}");

var m2 = await Post(adminAuth, "GstReturn/MonthSummary", new { PeriodKey = lastMonth });
decimal expectedConfirmed = bills.Take(2).Sum(b => b.IGSTTotal + b.CGSTTotal + b.SGSTTotal);
Console.WriteLine($"        after 2B: confirmed {D(m2.data, "CreditConfirmed")}, waiting {D(m2.data, "CreditWaiting")}, to pay {D(m2.data, "TotalPayable")}, carried forward {D(m2.data, "ClosingCredit")}");
Check("After 2B: the matched credit is confirmed", D(m2.data, "CreditConfirmed") == expectedConfirmed, $"{D(m2.data, "CreditConfirmed")} vs {expectedConfirmed}");
Check("After 2B: IGST credit pays the CGST and SGST too — nothing left to pay",
    D(m2.data, "TotalPayable") == 0m && D(m2.data, "ClosingCredit") == expectedConfirmed - output,
    $"to pay {D(m2.data, "TotalPayable")}, carried {D(m2.data, "ClosingCredit")} (= {expectedConfirmed} − {output})");
Check("The bill the supplier did not file is still waiting, and named",
    D(m2.data, "CreditWaiting") > 0m
    && (m2.data?["Purchases"] as JArray).Any(p => (string)p["CreditStatus"] == "Supplier has not filed"));
Check("The bill in 2B that was never entered is named", (m2.data?["TwoBMissingBills"] as JArray)?.Count == 1);

var again2b = await Post(adminAuth, "GstReturn/Import2b", new { Json = twoB, PeriodKey = lastMonth });
Check("Uploading the same file again claims nothing twice", again2b.ok && (int?)again2b.data?["CreditsClaimed"] == 0, $"claimed {again2b.data?["CreditsClaimed"]}");

var m3 = await Post(adminAuth, "GstReturn/MonthSummary", new { PeriodKey = thisMonth });
Check("The credit left over is brought into the next month", D(m3.data, "OpeningCredit") == D(m2.data, "ClosingCredit"),
    $"opening {D(m3.data, "OpeningCredit")} vs closing {D(m2.data, "ClosingCredit")}");

var g3 = await Post(adminAuth, "GstReturn/Gstr3b", new { CompanyGSTIN = companyGstin, PeriodKey = lastMonth, B2CLThreshold = 0 });
Check("The detailed GSTR-3B agrees with the summary", g3.ok && D(g3.data, "TotalPayable") == D(m2.data, "TotalPayable"),
    $"3B payable {D(g3.data, "TotalPayable")}");

var claimedBill = bills[0];
var lockedEdit = await Post(adminAuth, "Purchase/Update", new
{
    TaxInvoiceHeaderID = claimedBill.TaxInvoiceHeaderID, SupplierName = claimedBill.PartyName, SupplierGSTIN = claimedBill.PartyGSTIN,
    DocumentNo = claimedBill.DocumentNo, DocumentDate = claimedBill.DocumentDate,
    Lines = new[] { new { Description = "x", TaxableValue = 1m } },
});
Check("A bill whose credit was claimed can no longer be edited", !lockedEdit.ok && lockedEdit.message.Contains("claimed"), lockedEdit.message);
var lockedDel = await Post(adminAuth, "Purchase/Delete", new { TaxInvoiceHeaderID = claimedBill.TaxInvoiceHeaderID });
Check("…or deleted", !lockedDel.ok && lockedDel.message.Contains("claimed"), lockedDel.message);

var del = await Post(adminAuth, "Purchase/Delete", new { TaxInvoiceHeaderID = newId });
var list1 = await Post(adminAuth, "Purchase/Select", new { PeriodFrom = "2026-04-01", PeriodTo = "2027-03-31" });
Check("An unclaimed bill can be deleted, and leaves the list", del.ok && ((list1.data as JArray)?.Count ?? -1) == before, $"{del.message} {(list1.data as JArray)?.Count} bills (was {before})");

// ── The accountant ──────────────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("Accountant");
const string NoPermission = "permission";
if (accountant == null) Console.WriteLine("  (no accountant user in this database — skipped)");
else
{
    var acc = SignIn(accountant, "Accountant");
    foreach (var (path, body) in new (string, object)[]
    {
        ("Purchase/Select", new { PeriodFrom = "2026-04-01", PeriodTo = "2027-03-31" }),
        ("Purchase/Lookups", new { }),
        ("Purchase/GetById", new { TaxInvoiceHeaderID = claimedBill.TaxInvoiceHeaderID }),
        ("GstReturn/MonthSummary", new { PeriodKey = lastMonth }),
        ("TaxInvoice/Select", new { PeriodFrom = "2026-04-01", PeriodTo = "2027-03-31", Direction = "SALE", DocumentType = "", PartyUserID = 0 }),
    })
    {
        var r = await Post(acc, path, body);
        Check($"reads {path}", r.status == 200 && r.ok, $"{r.status} {r.message}");
    }
    foreach (var (path, body) in new (string, object)[]
    {
        ("Purchase/Capture", new { DocumentNo = "X", DocumentDate = "2026-11-01", Lines = new[] { new { TaxableValue = 1m } } }),
        ("Purchase/Update", new { TaxInvoiceHeaderID = 1 }),
        ("Purchase/Delete", new { TaxInvoiceHeaderID = 1 }),
        ("Purchase/SaveItem", new { Name = "X" }),
        ("Purchase/DeleteItem", new { PurchaseItemMasterID = 1 }),
        ("GstReturn/Import2b", new { Json = twoB, PeriodKey = lastMonth }),
        ("TaxInvoice/GenerateMonth", new { PeriodKey = lastMonth, DryRun = true }),
    })
    {
        var r = await Post(acc, path, body);
        Check($"cannot {path}", r.status == 403 || r.message.Contains(NoPermission, StringComparison.OrdinalIgnoreCase), $"{r.status} {r.message}");
    }
}

// ── The menu ────────────────────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("Menu");
var menu = FintechApp.Services.MenuService.Catalog();
string[] gone = { "/EInvoiceQueue", "/CreditNoteAlerts", "/GstCollection", "/Gstr2bRecon", "/ItcRegister", "/ChartOfAccounts", "/LedgerVouchers", "/TrialBalance", "/CommissionTax", "/TaxSimulator" };
foreach (string url in gone)
    Check($"{url} is off the menu", !menu.Any(e => string.Equals(e.Url, url, StringComparison.OrdinalIgnoreCase)));
Check("Purchase Register is now called Purchase Invoice", menu.Any(e => e.Url == "/PurchaseRegister" && e.Name == "Purchase Invoice"));
foreach (string url in new[] { "/AdminAccounting", "/AdminLedger" })
{
    var e = menu.First(x => x.Url == url);
    Check($"{e.Name} sits under Administration, for the admin", e.Group == "Administration" && e.Roles.SequenceEqual(new[] { 1 }));
}
foreach (string url in new[] { "/Invoice", "/TaxInvoices", "/PurchaseRegister", "/GstReturns" })
    Check($"{url} is on the Accountant menu", menu.Any(e => e.Url == url && e.Group == "Accountant"));

Console.WriteLine();
Console.WriteLine($"{passed} passed, {failed} failed");
return failed == 0 ? 0 : 1;
