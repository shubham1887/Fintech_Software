using System.Globalization;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Requests.TaxEngine;
using FintechApp.Services;
using FintechApp.StaticModels;

// Masters as exported from the seeded database with sqlcmd. The engine under test is the real
// TaxEngineService; only the repository is stood in for, so nothing about the resolution or the
// arithmetic is simulated.
string dir = Path.Combine(AppContext.BaseDirectory, "data");
if (!Directory.Exists(dir)) dir = Path.Combine(Directory.GetCurrentDirectory(), "data");

static decimal D(string s) => decimal.Parse(s.Trim() == "" ? "0" : s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture);
static DateTime Dt(string s) => DateTime.ParseExact(s.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
static DateTime? DtN(string s) => s.Trim() == "-" ? null : Dt(s);
static string Txt(string s) => s.Trim() == "-" ? string.Empty : s.Trim();
static string[][] Rows(string path) => File.ReadAllLines(path)
    .Where(l => l.Trim().Length > 0).Select(l => l.Split('|')).ToArray();

var rates = Rows(Path.Combine(dir, "rates.psv")).Select(c => new TaxRateMaster
{
    TaxRateMasterID = int.Parse(c[0]), TaxType = c[1], Code = c[2],
    Rate = D(c[3]), CessRate = D(c[4]),
    EffectiveFrom = Dt(c[5]), EffectiveTo = DtN(c[6]), IsActive = c[7].Trim() == "1",
}).ToList();

var sacs = Rows(Path.Combine(dir, "sacs.psv")).Select(c => new HSNSACMaster
{
    HSNSACMasterID = int.Parse(c[0]), Code = c[1], EffectiveFrom = Dt(c[2]), EffectiveTo = DtN(c[3]),
}).ToList();

var sections = Rows(Path.Combine(dir, "sections.psv")).Select(c => new TDSSectionMaster
{
    TDSSectionMasterID = int.Parse(c[0]), SectionCode = c[1], Rate = D(c[2]),
    ThresholdAnnual = D(c[3]), NoPANRate = D(c[4]), NonFilerRate = D(c[5]),
    EffectiveFrom = Dt(c[6]), EffectiveTo = DtN(c[7]),
}).ToList();

var rules = Rows(Path.Combine(dir, "rules.psv")).Select(c => new TaxRuleMaster
{
    TaxRuleMasterID = int.Parse(c[0]), Direction = c[1].Trim(),
    ServiceID = int.Parse(c[2]), OperatorID = int.Parse(c[3]), ProviderID = int.Parse(c[4]),
    UserTypeID = int.Parse(c[5]), PartyUserID = int.Parse(c[6]),
    PartyRegistrationStatus = Txt(c[7]), SupplyNature = Txt(c[8]),
    BillingBasis = c[9].Trim(),
    TaxRateMasterID = int.Parse(c[10]) == 0 ? null : int.Parse(c[10]),
    HSNSACMasterID = int.Parse(c[11]) == 0 ? null : int.Parse(c[11]),
    TDSSectionMasterID = int.Parse(c[12]) == 0 ? null : int.Parse(c[12]),
    ReverseCharge = c[13].Trim() == "1", ITCEligible = c[14].Trim() == "1",
    Priority = int.Parse(c[15]),
    EffectiveFrom = Dt(c[16]), EffectiveTo = DtN(c[17]), IsActive = c[18].Trim() == "1",
    RuleName = c[19].Trim(),
}).ToList();

var services = Rows(Path.Combine(dir, "services.psv"))
    .Select(c => (Id: int.Parse(c[0]), Category: Txt(c[1]), Name: c[2].Trim())).ToList();

Console.WriteLine($"loaded: {rules.Count} rules, {rates.Count} rates, {sacs.Count} SAC codes, {sections.Count} TDS sections\n");

var repo = new FileRepo(rates, sacs, sections, rules);
var engine = new TaxEngineService(repo);

int Svc(string category) => services.Where(s => s.Category == category).Select(s => s.Id).FirstOrDefault();
int recharge = services.Where(s => s.Name == "Prepaid Recharge").Select(s => s.Id).FirstOrDefault();
int dmt = Svc("Money Transfer");
int payout = Svc("Merchant Solutions");
int verify = Svc("Verification Services");

int bad = 0;
void Expect(string what, decimal got, decimal want)
{
    bool ok = got == want;
    if (!ok) bad++;
    Console.WriteLine($"   {what,-24} got {got,14:N2}   want {want,14:N2}   {(ok ? "ok" : "** MISMATCH **")}");
}
void Say(string what, string got, string want)
{
    bool ok = string.Equals(got, want, StringComparison.OrdinalIgnoreCase);
    if (!ok) bad++;
    Console.WriteLine($"   {what,-24} got {got,14}   want {want,14}   {(ok ? "ok" : "** MISMATCH **")}");
}

var d = new DateTime(2026, 9, 30);

// ── Ezytm Technologies, invoice ETPL/26-27/686 ──────────────────────────────────────
// Jaipur (08) -> Panvel (27). SAC 998413. Taxable 4,66,101.69 + IGST 83,898.30
// + Round Off 0.01 = 5,50,000.00
Console.WriteLine("1. Ezytm ETPL/26-27/686 - recharge top-up Rs 5,50,000 INCLUSIVE, 08 -> 27");
var r1 = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Purchase, ServiceID = recharge, TransactionDate = d,
    GrossValue = 550000m, IsTaxInclusive = true,
    SupplierStateCode = "08", PlaceOfSupplyStateCode = "27",
    PartyRegistrationStatus = PartyRegistrationStatus.Registered,
});
Console.WriteLine($"   rule '{r1.RuleName}'  basis={r1.BillingBasis}  nature={r1.SupplyNature}");
Say("SAC", r1.SACCode, "998413");
Say("basis", r1.BillingBasis, BillingBasis.Gross);
Say("supply nature", r1.SupplyNature, SupplyNature.Interstate);
Expect("taxable", r1.TaxableValue, 466101.69m);
Expect("IGST", r1.IGSTAmount, 83898.30m);
Expect("CGST", r1.CGSTAmount, 0m);
Expect("SGST", r1.SGSTAmount, 0m);
Expect("round off", r1.RoundOff, 0.01m);
Expect("invoice total", r1.InvoiceValue, 550000.00m);

// ── Erech Trade API LLP, e-invoice 2026-27/09/266 ───────────────────────────────────
// Gurugram (06) -> Panvel (27). HSN 998413. Taxable 18,01,694.92 + IGST 3,24,305.08
// = 21,26,000.00 (their invoice shows no round-off line).
Console.WriteLine("\n2. Erech 2026-27/09/266 - e-topup Rs 21,26,000 INCLUSIVE, 06 -> 27");
var r2 = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Purchase, ServiceID = recharge, TransactionDate = d,
    GrossValue = 2126000m, IsTaxInclusive = true,
    SupplierStateCode = "06", PlaceOfSupplyStateCode = "27",
    PartyRegistrationStatus = PartyRegistrationStatus.Registered,
});
Expect("taxable", r2.TaxableValue, 1801694.92m);
Say("supply nature", r2.SupplyNature, SupplyNature.Interstate);
Expect("taxable+tax+roundoff", r2.InvoiceValue, 2126000.00m);
Console.WriteLine($"   IGST {r2.IGSTAmount:N2} + round off {r2.RoundOff:N2}; supplier printed 3,24,305.08 with no round-off line");

// ── JaiJui Fintech, invoice JAP2627000002775 ────────────────────────────────────────
// Pune (27) -> Panvel (27), so CGST 9% + SGST 9%.
// Payout line:       1,506.00 throughput, 15.00 taxable, CGST 1.35 + SGST 1.35
// Verification line:     2.00 throughput,  6.00 taxable, CGST 0.54 + SGST 0.54
Console.WriteLine("\n3. JaiJui JAP2627000002775 - payout charge Rs 15.00 EXCLUSIVE, 27 -> 27");
var r3 = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Purchase, ServiceID = payout, TransactionDate = d,
    GrossValue = 1506m, ChargeValue = 15.00m, IsTaxInclusive = false,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRegistrationStatus = PartyRegistrationStatus.Registered,
});
Say("SAC", r3.SACCode, "997112");
Say("basis", r3.BillingBasis, BillingBasis.Charge);
Say("supply nature", r3.SupplyNature, SupplyNature.Intrastate);
Expect("taxable", r3.TaxableValue, 15.00m);
Expect("CGST", r3.CGSTAmount, 1.35m);
Expect("SGST", r3.SGSTAmount, 1.35m);
Expect("IGST", r3.IGSTAmount, 0m);
Expect("gross carried through", r3.GrossValue, 1506.00m);

Console.WriteLine("\n4. JaiJui same invoice - verification charge Rs 6.00 EXCLUSIVE, 27 -> 27");
var r4 = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Purchase, ServiceID = verify, TransactionDate = d,
    GrossValue = 2m, ChargeValue = 6.00m, IsTaxInclusive = false,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRegistrationStatus = PartyRegistrationStatus.Registered,
});
Say("SAC", r4.SACCode, "997159");
Expect("taxable", r4.TaxableValue, 6.00m);
Expect("CGST", r4.CGSTAmount, 0.54m);
Expect("SGST", r4.SGSTAmount, 0.54m);

// ── The basis separation, which is the whole point ──────────────────────────────────
Console.WriteLine("\n5. DMT sale - Rs 1,506 throughput, Rs 20 commission (COMMISSION basis)");
var r5 = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Sale, ServiceID = dmt, TransactionDate = d,
    GrossValue = 1506m, CommissionValue = 20.00m, IsTaxInclusive = false,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRegistrationStatus = PartyRegistrationStatus.Registered,
});
Say("basis", r5.BillingBasis, BillingBasis.Commission);
Expect("taxable = commission", r5.TaxableValue, 20.00m);
Expect("gross still recorded", r5.GrossValue, 1506.00m);

Console.WriteLine("\n6. Recharge sale - Rs 10,000 wallet top-up INCLUSIVE, 27 -> 27 (GROSS basis)");
var r6 = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Sale, ServiceID = recharge, TransactionDate = d,
    GrossValue = 10000m, IsTaxInclusive = true,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRegistrationStatus = PartyRegistrationStatus.Registered,
});
Expect("taxable", r6.TaxableValue, 8474.58m);
Expect("CGST", r6.CGSTAmount, 762.71m);
Expect("SGST", r6.SGSTAmount, 762.71m);
Expect("invoice total", r6.InvoiceValue, 10000.00m);

// ── TDS: the Bharti Cellular distinction ────────────────────────────────────────────
// The seed deliberately wires NO rule to a TDS section until the CA answers, so this uses a
// synthetic rule to exercise the engine's TDS path.
Console.WriteLine("\n7. TDS path (synthetic rule on 194H; the seed wires none yet)");
int s194H = sections.First(s => s.SectionCode == "194H").TDSSectionMasterID;
rules.Add(new TaxRuleMaster
{
    TaxRuleMasterID = 9001, Direction = TaxDirection.Sale, ServiceID = dmt, BillingBasis = BillingBasis.Commission,
    TaxRateMasterID = rates.First(r => r.Code == "GST18").TaxRateMasterID,
    HSNSACMasterID = sacs.First(s => s.Code == "997112").HSNSACMasterID,
    TDSSectionMasterID = s194H, Priority = 500, EffectiveFrom = new DateTime(2017, 7, 1), IsActive = true,
    RuleName = "synthetic - DMT commission with 194H",
});

var agentOver = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Sale, ServiceID = dmt, TransactionDate = d, PartyUserID = 7,
    GrossValue = 1506m, CommissionValue = 1000m,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRelationship = PartyRelationship.Agent, PartyYtdCommission = 25000m, PartyHasValidPAN = true,
});
Console.WriteLine("   AGENT, YTD 25,000 (over the 20,000 threshold), valid PAN:");
Say("section", agentOver.TDSSectionCode, "194H");
Expect("TDS rate", agentOver.TDSRate, 2.00m);
Expect("TDS on 1,000", agentOver.TDSAmount, 20.00m);

var p2p = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Sale, ServiceID = dmt, TransactionDate = d, PartyUserID = 7,
    GrossValue = 1506m, CommissionValue = 1000m,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRelationship = PartyRelationship.PrincipalToPrincipal, PartyYtdCommission = 25000m, PartyHasValidPAN = true,
});
Console.WriteLine("   P2P, same numbers (Bharti Cellular - a discount, not commission):");
Expect("TDS", p2p.TDSAmount, 0m);

var under = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Sale, ServiceID = dmt, TransactionDate = d, PartyUserID = 7,
    GrossValue = 1506m, CommissionValue = 1000m,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRelationship = PartyRelationship.Agent, PartyYtdCommission = 5000m, PartyHasValidPAN = true,
});
Console.WriteLine("   AGENT, YTD 5,000 + 1,000 = 6,000 (under the annual threshold):");
Expect("TDS", under.TDSAmount, 0m);

var noPan = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Sale, ServiceID = dmt, TransactionDate = d, PartyUserID = 7,
    GrossValue = 1506m, CommissionValue = 1000m,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRelationship = PartyRelationship.Agent, PartyYtdCommission = 25000m, PartyHasValidPAN = false,
});
Console.WriteLine("   AGENT, no PAN (s.206AA):");
Expect("TDS rate", noPan.TDSRate, 20.00m);
Expect("TDS on 1,000", noPan.TDSAmount, 200.00m);

var unknown = await engine.Resolve(new TaxResolutionContext
{
    Direction = TaxDirection.Sale, ServiceID = dmt, TransactionDate = d, PartyUserID = 7,
    GrossValue = 1506m, CommissionValue = 1000m,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
    PartyRelationship = "", PartyYtdCommission = 25000m, PartyHasValidPAN = true,
});
Console.WriteLine("   relationship UNKNOWN - must deduct nothing and warn:");
Expect("TDS", unknown.TDSAmount, 0m);
if (unknown.Warning.Contains("relationship", StringComparison.OrdinalIgnoreCase))
    Console.WriteLine("   warning present            ok");
else { bad++; Console.WriteLine("   warning MISSING            ** MISMATCH **"); }

// ── No rule at all ──────────────────────────────────────────────────────────────────
Console.WriteLine("\n8. Unknown direction - no rule can match, must warn and charge nothing");
var none = await engine.Resolve(new TaxResolutionContext
{
    Direction = "NOSUCH", ServiceID = recharge, TransactionDate = d, GrossValue = 1000m,
    SupplierStateCode = "27", PlaceOfSupplyStateCode = "27",
});
Expect("taxable", none.TaxableValue, 0m);
Expect("tax", none.TotalTax, 0m);
if (none.Warning.Length > 0) Console.WriteLine($"   warned: {none.Warning[..Math.Min(70, none.Warning.Length)]}...");
else { bad++; Console.WriteLine("   warning MISSING ** MISMATCH **"); }

Console.WriteLine();
Console.WriteLine(bad == 0 ? "ALL CHECKS PASSED" : $"{bad} CHECK(S) FAILED");
return bad == 0 ? 0 : 1;

/// <summary>Serves the masters exported from the seeded database. The engine is the real one.</summary>
sealed class FileRepo : ITaxConfigurationRepository
{
    private readonly List<TaxRateMaster> _rates;
    private readonly List<HSNSACMaster> _sacs;
    private readonly List<TDSSectionMaster> _sections;
    private readonly List<TaxRuleMaster> _rules;

    public FileRepo(List<TaxRateMaster> rates, List<HSNSACMaster> sacs,
                    List<TDSSectionMaster> sections, List<TaxRuleMaster> rules)
    { _rates = rates; _sacs = sacs; _sections = sections; _rules = rules; }

    public Task<List<TaxRateMaster>> GetRates() => Task.FromResult(_rates);
    public Task<List<HSNSACMaster>> GetHSNSAC() => Task.FromResult(_sacs);
    public Task<List<TDSSectionMaster>> GetTDSSections() => Task.FromResult(_sections);
    public Task<List<TaxRuleMaster>> GetRules() => Task.FromResult(_rules);
    public Task<List<PartyServiceRelationship>> GetPartyServiceRelationships() =>
        Task.FromResult(new List<PartyServiceRelationship>());
    public Task<PartyTaxProfile> GetPartyTaxProfile(int userId) => Task.FromResult<PartyTaxProfile>(null);
    public void InvalidateCache() { }
}
