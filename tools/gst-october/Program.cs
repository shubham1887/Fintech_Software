// gst-october — records the three October 2026 supplier bills and closes the GST month.
//
// The three bills were handed over as PDFs and are typed in here rather than parsed, because two
// of them are scans of a print-out and the third is an IRP extract whose JSON we were never given.
// Typing them into source has one property that typing them into a screen does not: the figures
// are reviewable, re-runnable and in version control beside the code that files them. Every
// number below is transcribed from the document named in its comment, including the sign of the
// round-off, and the totals are asserted against the supplier's own printed total before anything
// is sent — a transcription error here becomes a wrong return, and a wrong return becomes a
// notice.
//
// What it does, in order:
//   1. Captures the three purchases (idempotent on supplier + document number — a re-run adds
//      nothing and says so).
//   2. Builds the ITC register for the period, so the credit on those bills is claimable.
//   3. Generates our own October sale documents from what actually happened — commission and
//      charge first, then the gross-basis wallet top-ups.
//   4. Builds GSTR-1 and GSTR-3B for the month and prints them, including the 1-vs-3B variance.
//
// It drives the RUNNING API over HTTP as a signed-in admin, so every figure goes through the JWT
// filter, the access policy and the real services — the same path the screens use.
//
//   dotnet run --project tools/gst-october -- <baseUrl> <database> <settingsFolder> [--commit]
//
// Without --commit the sale-document generation is a DRY RUN: it reports what it would issue and
// writes nothing. Purchases are captured either way (recording a supplier's bill allocates no
// number of ours and puts nothing into GSTR-1), which is what makes a dry run worth reading.

using System.Globalization;
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
bool commit = args.Contains("--commit", StringComparer.OrdinalIgnoreCase);

// --regroup <TRANSACTION|DAILY|MONTHLY>
//
// Re-issues a month that is ALREADY invoiced under a different grouping: the existing documents
// are cancelled (numbers kept, declared in GSTR-1 table 13) and replaced. Needs --commit, because
// unlike everything else here it withdraws documents that exist.
string regroup = string.Empty;
{
    int i = Array.FindIndex(args, a => a.Equals("--regroup", StringComparison.OrdinalIgnoreCase));
    if (i >= 0)
    {
        regroup = (args.ElementAtOrDefault(i + 1) ?? string.Empty).Trim().ToUpperInvariant();
        if (regroup is not ("TRANSACTION" or "DAILY" or "MONTHLY"))
        {
            Console.WriteLine("--regroup needs one of TRANSACTION, DAILY or MONTHLY.");
            return 2;
        }
    }
}

// --set-state <userId>=<twoDigitGstCode>, repeatable.
//
// A place of supply is a fact about somebody else's business, and this tool will not invent one:
// where nothing on record can derive it, it has to be typed by a person who knows the answer.
// This is how that answer is given, and it is applied BEFORE any document is issued, because a
// document keeps the code it was stamped with and correcting it later means cancelling and
// re-issuing.
var setStates = new List<(int UserId, string Code)>();
foreach (string a in args.Where(x => x.StartsWith("--set-state", StringComparison.OrdinalIgnoreCase)))
{
    string v = a.Contains('=') ? a[(a.IndexOf('=') + 1)..] : "";
    // Also accept "--set-state 2=27" as two arguments.
    if (v.Length == 0) v = args.SkipWhile(x => !ReferenceEquals(x, a)).Skip(1).FirstOrDefault() ?? "";
    var bits = v.Split(':', '=');
    if (bits.Length == 2 && int.TryParse(bits[0], out int uid) && bits[1].Trim().Length is 1 or 2)
        setStates.Add((uid, bits[1].Trim().PadLeft(2, '0')));
    else { Console.WriteLine($"Could not read {a} — expected --set-state <userId>=<stateCode>."); return 2; }
}

if (db.Length == 0)
{
    Console.WriteLine("Usage: gst-october <baseUrl> <database> <settingsFolder> [--commit] [--set-state <userId>=<code> ...]");
    return 2;
}

// ── Our own registration, from the bills' "Buyer" block ──────────────────────────────────
const string CompanyGstin = "27ABCCS3656G1Z8";   // SAMS STRATEGY PRIVATE LIMITED
const string CompanyState = "27";                // Maharashtra
const string PeriodKey = "202610";               // October 2026
var periodFrom = new DateTime(2026, 10, 1);
var periodTo = new DateTime(2026, 10, 31);

// ── The three bills, as printed ──────────────────────────────────────────────────────────

var bills = new List<Bill>
{
    // ERECH TRADE API LLP — the recharge exchange. IRP extract, doc 2026-27/10/237 dated
    // 31/10/2026, ack 132629704489751 on 05-11-2026. Haryana (06) to Maharashtra (27), so the
    // supplier charged IGST: one line, HSN 998413 "E TOP". The printed summary block reads
    // taxable 1162463.56, IGST 209243.44, round off 0, total 1371707.00.
    new Bill
    {
        SupplierName  = "ERECH TRADE API LLP",
        SupplierGstin = "06AAJFE9501B1ZT",
        SupplierState = "06",
        DocumentNo    = "2026-27/10/237",
        DocumentDate  = new DateTime(2026, 10, 31),
        RoundOff      = 0m,
        PrintedTotal  = 1371707.00m,
        Remark        = "Recharge stock (E TOP) — IRN ack 132629704489751, 05-11-2026",
        Lines =
        {
            new Line("E TOP — recharge stock", "998413", 1162463.56m, igst: 209243.44m, igstRate: 18m),
        },
    },

    // EZYTM TECHNOLOGIES PVT LTD — the "robotic" recharge bill. e-Invoice ETPL/26-27/799 dated
    // 31-Oct-26, ack 172621602163546 on 4-Nov-26. Rajasthan (08) to Maharashtra (27): IGST again.
    // One line, HSN 998413 "Recharge": taxable 33898.31, IGST 6101.70, and a round-off of −0.01
    // that takes 40000.01 down to the 40000.00 the invoice is actually for.
    new Bill
    {
        SupplierName  = "EZYTM TECHNOLOGIES PVT LTD",
        SupplierGstin = "08AAFCE0827P1ZB",
        SupplierState = "08",
        DocumentNo    = "ETPL/26-27/799",
        DocumentDate  = new DateTime(2026, 10, 31),
        RoundOff      = -0.01m,
        PrintedTotal  = 40000.00m,
        Remark        = "Recharge stock — e-invoice, IRN ack 172621602163546, 4-Nov-26",
        Lines =
        {
            new Line("Recharge", "998413", 33898.31m, igst: 6101.70m, igstRate: 18m),
        },
    },

    // JAIJUI FINTECH PRIVATE LIMITED — the DMT/payout side. Invoice JAP2627000003405 dated
    // 31 October 2026, dealer code API13178. Maharashtra to Maharashtra, place of supply
    // Maharashtra, so this one is CGST + SGST and not IGST.
    //
    // Two lines, and the second is not a mistake: the VERIFICATION line is billed on a NEGATIVE
    // commission (−2.54) and still carries 3.00 of taxable value and 0.54 of tax. It is typed as
    // the invoice prints it.
    //
    // The round-off of +0.01 is the invoice's own inconsistency, not ours, and it is worth
    // spelling out. The payout line prints CGST 347.66 and SGST 347.66 but a GST column of
    // 695.33 — because 3862.92 at 9% is 347.6628, which rounds DOWN to 347.66 per head and the
    // pair of them rounds UP to 695.33. So the four tax heads add to 695.86 while the invoice
    // declares 695.87 and an invoice value of 4561.79.
    //
    // The heads are kept exactly as the supplier printed them, because those are the figures
    // they will have filed and the figures GSTR-2B will be matched on. The paisa they do not
    // account for is carried as a round-off, which is what makes the stored document equal the
    // 4561.79 that will actually be paid. Inventing a 347.67 to make the arithmetic tidy would
    // have put our books a paisa away from their return on the head that is reconciled.
    new Bill
    {
        SupplierName  = "JAIJUI FINTECH PRIVATE LIMITED",
        SupplierGstin = "27AAFCJ8090F1ZC",
        SupplierState = "27",
        DocumentNo    = "JAP2627000003405",
        DocumentDate  = new DateTime(2026, 10, 31),
        RoundOff      = 0.01m,
        PrintedTotal  = 4561.79m,
        Remark        = "DMT / payout charges — dealer API13178, 285 payouts on 19,49,352.00",
        Lines =
        {
            new Line("Payout charge", "997112", 3862.92m, cgst: 347.66m, sgst: 347.66m, cgstRate: 9m, sgstRate: 9m),
            new Line("Verification", "997159", 3.00m, cgst: 0.27m, sgst: 0.27m, cgstRate: 9m, sgstRate: 9m),
        },
    },
};

// ── Transcription check, before a single request is sent ─────────────────────────────────
//
// Each bill's typed lines must add up to the total printed on it. A bill that does not is a
// typing mistake, and the only safe thing to do with a typing mistake in a tax figure is refuse
// to file it.
bool transcriptionOk = true;
Console.WriteLine("Transcription check — typed lines against the printed total");
foreach (var b in bills)
{
    decimal computed = Math.Round(b.Taxable + b.Tax + b.RoundOff, 2);
    bool ok = computed == b.PrintedTotal;
    transcriptionOk &= ok;
    Console.WriteLine($"  {(ok ? "ok  " : "FAIL")}  {b.SupplierName,-32} {b.DocumentNo,-18} "
                    + $"taxable {b.Taxable,14:N2}  tax {b.Tax,12:N2}  round {b.RoundOff,6:N2}  "
                    + $"= {computed,14:N2}  printed {b.PrintedTotal,14:N2}");
}
if (!transcriptionOk) { Console.WriteLine("\nRefusing to continue: a bill does not add up to its printed total."); return 3; }
Console.WriteLine();

// ── The same configuration the running instance loaded ───────────────────────────────────
//
// appsettings.json then appsettings.Production.json on top, which is the order the host itself
// uses — the live folder keeps its encryption key and JWT secret only in the second file, so
// reading just the first would mint a token the API refuses and an envelope it cannot decrypt.
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

var admin = await ctx.UserMasters.AsNoTracking()
    .FirstOrDefaultAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Admin);
if (admin == null) { Console.WriteLine($"No admin user in {db}."); return 4; }

Console.WriteLine($"API       : {baseUrl}");
Console.WriteLine($"Database  : {db}");
Console.WriteLine($"As        : {admin.FirmName} (#{admin.UserMasterID}), Admin");
Console.WriteLine($"Period    : {PeriodKey}  ({periodFrom:dd MMM yyyy} – {periodTo:dd MMM yyyy})");
Console.WriteLine($"Mode      : {(commit ? "COMMIT — sale documents will be issued" : "DRY RUN — sale documents reported only")}");
Console.WriteLine();

var tokens = jwt.GenerateJwtToken(admin.UserMasterID, UserTypeCode.Admin, "Admin",
    admin.MobileNumber, admin.EmailID, admin.SessionEpoch);

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

string keyData = crypto.Encrypt(JsonConvert.SerializeObject(new
{
    UserID = admin.UserMasterID,
    UserTypeID = UserTypeCode.Admin,
    // The envelope has to name the same identity as the bearer token - the API refuses a body
    // whose KeyData and token disagree, which is what stops one signed-in user acting as another.
    SessionToken = tokens.AccessToken,
    DeviceType = "Web",
    IPAddress = "127.0.0.1",
}));

async Task<JToken> Post(string path, object body)
{
    using var form = new MultipartFormDataContent
    {
        { new StringContent(crypto.Encrypt(JsonConvert.SerializeObject(body))), "Request" },
        { new StringContent(keyData), "KeyData" },
    };
    var response = await http.PostAsync($"{baseUrl}/api/{path}", form);
    string raw = await response.Content.ReadAsStringAsync();

    JObject envelope;
    try { envelope = JObject.Parse(raw); }
    catch { throw new Exception($"{path}: HTTP {(int)response.StatusCode}, body was not JSON: {Trim(raw)}"); }

    // The API answers in its own envelope: a status, a message, and the payload in Data.
    string status = envelope["Status"]?.ToString() ?? envelope["status"]?.ToString() ?? "";
    string message = envelope["Message"]?.ToString() ?? envelope["message"]?.ToString() ?? "";
    string payload = envelope["Data"]?.ToString() ?? envelope["data"]?.ToString() ?? "";

    if (!status.Equals("Success", StringComparison.OrdinalIgnoreCase) &&
        !status.Equals("Ok", StringComparison.OrdinalIgnoreCase) && status != "1")
        throw new Exception($"{path}: {status} — {message}");

    if (payload.Length == 0) return new JObject();

    // Some controllers encrypt the payload and some return it in the clear — the tax controllers
    // go through TaxControllerBase, which encrypts; the older ones serialise straight into Data.
    // Try it as JSON first and only decrypt what is not, so the tool reads either without being
    // told which is which.
    return Parse(payload) ?? Parse(crypto.Decrypt(payload))
        ?? throw new Exception($"{path}: the payload was neither JSON nor decryptable JSON.");

    static JToken? Parse(string s)
    {
        s = (s ?? "").TrimStart();
        if (s.Length == 0) return null;
        try
        {
            if (s.StartsWith('[')) return JArray.Parse(s);
            if (s.StartsWith('{')) return JObject.Parse(s);
        }
        catch { /* not JSON — the caller falls through to the other reading */ }
        return null;
    }
}

static string Trim(string s) => s.Length <= 300 ? s : s[..300] + "…";
static string Money(JToken t) => (t?.Value<decimal>() ?? 0m).ToString("N2", CultureInfo.InvariantCulture);

int exit = 0;

// ── 1. The three supplier bills ──────────────────────────────────────────────────────────
Console.WriteLine("1. Purchases");
foreach (var b in bills)
{
    try
    {
        var r = await Post("Purchase/Capture", new
        {
            SupplierUserID = 0,
            SupplierName = b.SupplierName,
            SupplierGSTIN = b.SupplierGstin,
            SupplierStateCode = b.SupplierState,
            CompanyGSTIN = CompanyGstin,
            PlaceOfSupplyStateCode = CompanyState,
            DocumentNo = b.DocumentNo,
            DocumentDate = b.DocumentDate,
            WalletCreditDate = (DateTime?)null,
            ProviderID = 0,
            WalletID = 0,
            RoundOff = b.RoundOff,
            Remark = b.Remark,
            Lines = b.Lines.Select(l => new
            {
                ServiceID = 0,
                Description = l.Description,
                SACCode = l.Sac,
                Quantity = 1,
                TaxableValue = l.Taxable,
                CGSTRate = l.CgstRate, CGSTAmount = l.Cgst,
                SGSTRate = l.SgstRate, SGSTAmount = l.Sgst,
                IGSTRate = l.IgstRate, IGSTAmount = l.Igst,
                CessAmount = 0m,
                ITCEligible = true,
                ReverseCharge = false,
            }),
        });

        bool existed = r["AlreadyExisted"]?.Value<bool>() ?? false;
        decimal grand = r["GrandTotal"]?.Value<decimal>() ?? 0m;
        Console.WriteLine($"   {(existed ? "already captured" : "captured        ")}  {b.DocumentNo,-18} "
                        + $"{b.SupplierName,-32} {grand,14:N2}  ITC {Money(r["EligibleITC"])}");

        // The captured document must still equal the paper. Idempotency means a re-run returns the
        // stored document, so this also catches a bill that was first captured with wrong figures.
        if (grand != b.PrintedTotal)
        {
            Console.WriteLine($"     MISMATCH: stored total {grand:N2} is not the printed {b.PrintedTotal:N2}");
            exit = 5;
        }
        string warning = r["Warning"]?.ToString() ?? "";
        if (warning.Length > 0) Console.WriteLine($"     note: {warning}");
    }
    catch (Exception ex) { Console.WriteLine($"   FAILED  {b.DocumentNo}: {ex.Message}"); exit = 5; }
}
Console.WriteLine();

// ── 2. The credit those bills carry ──────────────────────────────────────────────────────
Console.WriteLine("2. ITC register");
try
{
    var r = await Post("Itc/Build", new { PeriodKey, CreatedByUserID = 0 });
    Console.WriteLine($"   created {r["EntriesCreated"]}  updated {r["EntriesUpdated"]}"
                    + $"  eligible credit {Money(r["TotalEligibleITC"])}");

    // Building the register is not claiming the credit, and the difference is the whole month's
    // cash: an unclaimed credit does not reach GSTR-3B table 4, so the return asks for tax to be
    // paid that the credit would have covered. Claim every eligible, unclaimed entry into this
    // period — the service refuses the ones that are not safe to claim and says which.
    var register = await Post("Itc/Register", new { PeriodKey, DaysAhead = 0 });
    var entries = (register as JArray) ?? (register["Rows"] as JArray) ?? new JArray();
    var toClaim = entries
        .Where(e => (e["ClaimedInPeriod"]?.ToString() ?? "").Length == 0
                 && (e["EligibleITC"]?.Value<decimal>() ?? 0) > 0)
        .Select(e => e["ItcRegisterEntryID"]!.Value<int>())
        .ToList();

    if (toClaim.Count == 0)
        Console.WriteLine("   nothing left to claim for this period.");
    else if (!commit)
        Console.WriteLine($"   would claim {toClaim.Count} entr{(toClaim.Count == 1 ? "y" : "ies")} into {PeriodKey}"
                        + $" — {Money(entries.Where(e => toClaim.Contains(e["ItcRegisterEntryID"]!.Value<int>())).Sum(e => e["EligibleITC"]?.Value<decimal>() ?? 0))} of credit.");
    else
    {
        var claim = await Post("Itc/Claim", new { EntryIDs = toClaim, ClaimPeriod = PeriodKey, CreatedByUserID = 0 });
        Console.WriteLine($"   claimed {claim["Claimed"]} of {toClaim.Count} into {PeriodKey}");
        string note = claim["Message"]?.ToString() ?? "";
        if (note.Length > 0) Console.WriteLine($"     note: {note}");
    }
}
catch (Exception ex) { Console.WriteLine($"   FAILED: {ex.Message}"); exit = 6; }
Console.WriteLine();

// ── 3. Place of supply, BEFORE anything is issued ────────────────────────────────────────
//
// A document is stamped with the party's state at the moment it is generated and keeps it for
// ever — correcting it afterwards means cancelling the document and re-issuing, which is a far
// worse afternoon than deriving the states first. A party with nothing on record is treated as
// intra-state, so getting this wrong sends real supplies to the wrong tax head.
Console.WriteLine($"3. Place of supply ({(commit ? "deriving" : "preview")})");
try
{
    var review = await Post("PartyState/Review", new { });
    Console.WriteLine($"   parties {review["Total"]}  already set {review["AlreadySet"]}"
                    + $"  derivable {review["CanBackfill"]}  need a human {review["NeedsAttention"]}"
                    + $"  documents already stamped badly {review["DocumentsWithBadPlaceOfSupply"]}");

    var fill = await Post("PartyState/Backfill", new { DryRun = !commit, OnlyUserID = 0 });
    Console.WriteLine($"   {(commit ? "created" : "would create")} {fill["ProfilesCreated"]}"
                    + $"  {(commit ? "updated" : "would update")} {fill["ProfilesUpdated"]}"
                    + $"  still unknown {fill["StillUnknown"]}");

    // Our OWN account is not a derivation and not a guess: a registration's place of supply is
    // the state it is registered in, and that is on the company profile. It is set here because
    // the resolver only reads a party's own record, and ours is blank — the company's details
    // live on the invoice settings instead.
    var self = await ctx.UserMasters.AsNoTracking()
        .FirstOrDefaultAsync(u => !u.IsDeleted && u.UserTypeID == UserTypeCode.Admin);
    if (self != null) setStates.Insert(0, (self.UserMasterID, CompanyState));

    foreach (var (userId, code) in setStates.DistinctBy(s => s.UserId))
    {
        if (!commit) { Console.WriteLine($"   would set #{userId} to {code}"); continue; }
        try
        {
            await Post("PartyState/SetPlaceOfSupply",
                new { UserID = userId, PlaceOfSupplyStateCode = code, CreatedByUserID = 0 });
            Console.WriteLine($"   set #{userId} to {code}");
        }
        catch (Exception ex) { Console.WriteLine($"   FAILED to set #{userId}: {ex.Message}"); exit = 7; }
    }

    // A party nothing can derive is not a failure of this run — it is a fact about the data, and
    // the only fix is somebody typing the state in. Name them, because "still unknown: 5" is not
    // something anyone can act on.
    foreach (var m in (fill["Messages"] as JArray) ?? new JArray())
        Console.WriteLine($"     {m}");

    foreach (var p in (review["Parties"] as JArray) ?? new JArray())
    {
        if (p["NeedsAttention"]?.Value<bool>() != true) continue;
        Console.WriteLine($"     needs a state by hand: #{p["UserID"]} {p["PartyName"]}"
                        + $" ({p["MobileNumber"]}){(p["GSTIN"]?.ToString().Length > 0 ? $", GSTIN {p["GSTIN"]}" : ", no GSTIN")}");
    }
}
catch (Exception ex) { Console.WriteLine($"   FAILED: {ex.Message}"); exit = 7; }
Console.WriteLine();

// ── 4. Our own October documents ─────────────────────────────────────────────────────────
//
// Always dry-run first, even when committing. The dry run is the only thing that can say WHICH
// parties actually receive a document this month, and that is the question that decides whether
// it is safe to issue: a document is stamped with its party's place of supply and keeps it, the
// portal rejects a blank one, and withdrawing it means a cancellation and a re-issue against a
// numbering series that has to stay demonstrable. A party with no state who receives nothing is
// not a problem; the same party receiving an invoice is. Only the dry run tells them apart.
string[] generators = { "TaxInvoice/GenerateCommission", "TaxInvoice/GenerateTopUps" };

Console.WriteLine("4. Sale documents");
var blockers = new List<string>();
foreach (string endpoint in generators)
{
    try
    {
        var r = await Post(endpoint, new
        {
            PeriodFrom = periodFrom, PeriodTo = periodTo,
            CompanyGSTIN = CompanyGstin, SupplierStateCode = CompanyState,
            DryRun = true, OnlyPartyUserID = 0,
        });
        Console.WriteLine($"   would issue  {endpoint.Split('/')[1],-20} documents {r["DocumentsGenerated"]}"
                        + $"  taxable {Money(r["TaxableTotal"])}  tax {Money(r["TaxTotal"])}");
        foreach (var w in (r["Warnings"] as JArray) ?? new JArray())
        {
            string text = w.ToString();
            if (text.Contains("No place of supply", StringComparison.OrdinalIgnoreCase)) blockers.Add(text);
            else Console.WriteLine($"     note: {text}");
        }
    }
    catch (Exception ex) { Console.WriteLine($"   FAILED  {endpoint}: {ex.Message}"); exit = 7; }
}

if (blockers.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("   These parties would receive an October document with NO place of supply:");
    foreach (string b in blockers.Distinct()) Console.WriteLine($"     - {b}");
    Console.WriteLine("   The portal rejects a blank place of supply, and a document already issued keeps the");
    Console.WriteLine("   code it was stamped with — so set the state first and run again:");
    Console.WriteLine("     --set-state <userId>=<twoDigitStateCode>   (27 = Maharashtra, 07 = Delhi, …)");
    Console.WriteLine("   or set it on the Place of Supply screen. Nothing was issued.");
}
else if (commit)
{
    foreach (string endpoint in generators)
    {
        try
        {
            var r = await Post(endpoint, new
            {
                PeriodFrom = periodFrom, PeriodTo = periodTo,
                CompanyGSTIN = CompanyGstin, SupplierStateCode = CompanyState,
                DryRun = false, OnlyPartyUserID = 0,
            });
            Console.WriteLine($"   ISSUED       {endpoint.Split('/')[1],-20} documents {r["DocumentsGenerated"]}"
                            + $"  lines {r["LinesGenerated"]}  taxable {Money(r["TaxableTotal"])}  tax {Money(r["TaxTotal"])}");
        }
        catch (Exception ex) { Console.WriteLine($"   FAILED  {endpoint}: {ex.Message}"); exit = 7; }
    }
}
else Console.WriteLine("   (dry run — pass --commit to issue)");
Console.WriteLine();

// ── 5. The month's returns ───────────────────────────────────────────────────────────────
// ── 4b. Re-issue the month under a different grouping ────────────────────────────────────
//
// Only when asked. This is the one step here that withdraws documents rather than adding them,
// so it never happens as part of an ordinary run.
if (regroup.Length > 0)
{
    Console.WriteLine();
    Console.WriteLine($"4b. Re-issue {PeriodKey} grouped {regroup} ({(commit ? "COMMIT" : "preview")})");
    try
    {
        var r = await Post("TaxInvoice/RegroupMonth", new
        {
            PeriodKey, DryRun = !commit, Consolidation = regroup, SaveAsDefault = commit,
        });

        if (r["Refused"]?.Value<bool>() == true)
        {
            Console.WriteLine($"   REFUSED: {r["RefusalReason"]}");
            exit = 10;
        }
        else
        {
            Console.WriteLine($"   cancelled {r["CancelledDocuments"] ?? 0}"
                            + $"  {(commit ? "issued" : "would issue")} {r["DocumentsGenerated"]}"
                            + $"  taxable {Money(r["TaxableTotal"])}  tax {Money(r["TaxTotal"])}");
        }
        foreach (var w in (r["Warnings"] as JArray) ?? new JArray())
            Console.WriteLine($"     note: {w}");
    }
    catch (Exception ex) { Console.WriteLine($"   FAILED: {ex.Message}"); exit = 10; }
}
Console.WriteLine();

Console.WriteLine("5. Returns");
try
{
    var g1 = await Post("GstReturn/Gstr1", new { CompanyGSTIN = CompanyGstin, PeriodKey, B2CLThreshold = 0m });
    decimal g1Tax = (g1["TotalIGST"]?.Value<decimal>() ?? 0) + (g1["TotalCGST"]?.Value<decimal>() ?? 0)
                  + (g1["TotalSGST"]?.Value<decimal>() ?? 0) + (g1["TotalCess"]?.Value<decimal>() ?? 0);
    Console.WriteLine($"   GSTR-1   documents {g1["TotalDocuments"]}"
                    + $"  taxable {Money(g1["TotalTaxableValue"])}"
                    + $"  IGST {Money(g1["TotalIGST"])}  CGST {Money(g1["TotalCGST"])}  SGST {Money(g1["TotalSGST"])}"
                    + $"  tax {g1Tax:N2}");
    Console.WriteLine($"            B2B {((g1["B2B"] as JArray)?.Count ?? 0)} parties"
                    + $"  B2CL {((g1["B2CL"] as JArray)?.Count ?? 0)}"
                    + $"  B2CS {((g1["B2CS"] as JArray)?.Count ?? 0)}"
                    + $"  HSN {((g1["HSN"] as JArray)?.Count ?? 0)}");
    foreach (var w in (g1["Warnings"] as JArray) ?? new JArray())
        Console.WriteLine($"     note: {w}");

    var g3 = await Post("GstReturn/Gstr3b", new { CompanyGSTIN = CompanyGstin, PeriodKey, B2CLThreshold = 0m });
    decimal output = (g3["OutputIGST"]?.Value<decimal>() ?? 0) + (g3["OutputCGST"]?.Value<decimal>() ?? 0)
                   + (g3["OutputSGST"]?.Value<decimal>() ?? 0) + (g3["OutputCess"]?.Value<decimal>() ?? 0);
    decimal itc = (g3["NetItcIGST"]?.Value<decimal>() ?? 0) + (g3["NetItcCGST"]?.Value<decimal>() ?? 0)
                + (g3["NetItcSGST"]?.Value<decimal>() ?? 0) + (g3["NetItcCess"]?.Value<decimal>() ?? 0);
    Console.WriteLine($"   GSTR-3B  output tax {output:N2}  ITC available {itc:N2}"
                    + $"  payable {Money(g3["TotalPayable"])}");
    Console.WriteLine($"            IGST {Money(g3["OutputIGST"])} / ITC {Money(g3["NetItcIGST"])}"
                    + $"   CGST {Money(g3["OutputCGST"])} / ITC {Money(g3["NetItcCGST"])}"
                    + $"   SGST {Money(g3["OutputSGST"])} / ITC {Money(g3["NetItcSGST"])}");
    // The mismatch a notice is most often written about, so it is printed every time rather than
    // only when somebody thinks to look.
    bool agrees = g3["AgreesWithGstr1"]?.Value<bool>() ?? false;
    Console.WriteLine($"   1-vs-3B  {(agrees ? "agrees" : "DISAGREES")}"
                    + $"  — taxable variance {Money(g3["VarianceTaxableValue"])}, tax variance {Money(g3["VarianceTax"])}");
    if (!agrees) exit = 9;
    foreach (var w in (g3["Warnings"] as JArray) ?? new JArray())
        Console.WriteLine($"     note: {w}");
}
catch (Exception ex) { Console.WriteLine($"   FAILED: {ex.Message}"); exit = 8; }

Console.WriteLine();
Console.WriteLine(exit == 0 ? "Done." : $"Finished with problems (exit {exit}).");
return exit;

// ── Shapes ───────────────────────────────────────────────────────────────────────────────

sealed class Line
{
    public Line(string description, string sac, decimal taxable,
                decimal cgst = 0, decimal sgst = 0, decimal igst = 0,
                decimal cgstRate = 0, decimal sgstRate = 0, decimal igstRate = 0)
    {
        Description = description; Sac = sac; Taxable = taxable;
        Cgst = cgst; Sgst = sgst; Igst = igst;
        CgstRate = cgstRate; SgstRate = sgstRate; IgstRate = igstRate;
    }

    public string Description { get; }
    public string Sac { get; }
    public decimal Taxable { get; }
    public decimal Cgst { get; }
    public decimal Sgst { get; }
    public decimal Igst { get; }
    public decimal CgstRate { get; }
    public decimal SgstRate { get; }
    public decimal IgstRate { get; }
}

sealed class Bill
{
    public string SupplierName { get; init; } = "";
    public string SupplierGstin { get; init; } = "";
    public string SupplierState { get; init; } = "";
    public string DocumentNo { get; init; } = "";
    public DateTime DocumentDate { get; init; }
    public decimal RoundOff { get; init; }
    /// <summary>The total printed on the bill, which the typed lines have to reproduce.</summary>
    public decimal PrintedTotal { get; init; }
    public string Remark { get; init; } = "";
    public List<Line> Lines { get; } = new();

    public decimal Taxable => Lines.Sum(l => l.Taxable);
    public decimal Tax => Lines.Sum(l => l.Cgst + l.Sgst + l.Igst);
}
