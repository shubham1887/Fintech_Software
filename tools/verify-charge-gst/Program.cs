using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderConfigurationAdminCommission;
using FintechApp.Responses.CommissionTax;
using FintechApp.Responses.ProviderConfigurationAdminCommission;
using FintechApp.Services;
using FintechApp.StaticModels;

// Verifies the per-band GST on a charge: the arithmetic against every figure in the written
// specification, and then the four-layer resolution through the REAL ChargeGstResolver — only the
// three sources it reads (the provider rate card, the slab, the commission tax profile) are stood
// in for, so nothing about the decision or the split is simulated.
//
//   dotnet run --project tools/verify-charge-gst

int pass = 0, fail = 0;

void Check(string what, decimal actual, decimal expected)
{
    bool ok = actual == expected;
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-62} {actual,12:0.00}   expected {expected,10:0.00}");
}

void CheckText(string what, string actual, string expected)
{
    bool ok = string.Equals(actual, expected, StringComparison.Ordinal);
    if (ok) pass++; else fail++;
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what,-62} {actual,12}   expected {expected,10}");
}

void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"── {title} " + new string('─', Math.Max(0, 76 - title.Length)));
}

Console.WriteLine("Charge GST — formula and four-layer resolution");
Console.WriteLine(new string('=', 80));

// ─────────────────────────────────────────────────────────────────────────────────────────────
// 1. The formula, against the specification's own figures.
// ─────────────────────────────────────────────────────────────────────────────────────────────
Section("Provider side, INCLUSIVE 18% (JaiJui's card divides by 1.18 cleanly)");

foreach (var (charge, gst, b) in new[]
{
    (3.54m, 0.54m, 3.00m),     // Verify 1-3
    (5.90m, 0.90m, 5.00m),     // DMT 500-1000
    (9.44m, 1.44m, 8.00m),     // DMT 1001-3000
    (13.57m, 2.07m, 11.50m),   // DMT 3001-5000
    (16.52m, 2.52m, 14.00m),   // DMT 5001-10000
    (11.80m, 1.80m, 10.00m),   // Sender KYC 1-4
})
{
    var s = ChargeGstResolver.Split(charge, ChargeGstType.Inclusive, 18m);
    Check($"{charge:0.00} inclusive -> GST", s.GSTAmount, gst);
    Check($"{charge:0.00} inclusive -> base", s.BaseCharge, b);
    Check($"{charge:0.00} inclusive -> total (unchanged)", s.TotalCharge, charge);
}

Section("Percentage band: 0.17% of 20,000, inclusive");
{
    decimal charge = 20000m * 0.17m / 100m;              // 34.00, as the card computes it
    var s = ChargeGstResolver.Split(charge, ChargeGstType.Inclusive, 18m);
    Check("0.17% of 20,000 -> charge", s.ChargeAsQuoted, 34.00m);
    Check("34.00 inclusive -> GST", s.GSTAmount, 5.19m);   // 34 x 18/118 = 5.1864 -> 5.19
    Check("34.00 inclusive -> base", s.BaseCharge, 28.81m);
}

Section("Retailer slab: the three bands of the written example");
{
    // 500-1000, Rs 8 inclusive
    var a = ChargeGstResolver.Split(8m, ChargeGstType.Inclusive, 18m);
    Check("Rs 8 inclusive -> GST", a.GSTAmount, 1.22m);     // 8 x 18/118 = 1.2203 -> 1.22
    Check("Rs 8 inclusive -> base", a.BaseCharge, 6.78m);

    // 1001-5000, Rs 10 exclusive
    var b = ChargeGstResolver.Split(10m, ChargeGstType.Exclusive, 18m);
    Check("Rs 10 exclusive -> GST", b.GSTAmount, 1.80m);
    Check("Rs 10 exclusive -> base (unchanged)", b.BaseCharge, 10.00m);
    Check("Rs 10 exclusive -> total payable", b.TotalCharge, 11.80m);

    // 5001-25000, Rs 12 no GST
    var c = ChargeGstResolver.Split(12m, ChargeGstType.None, 18m);
    Check("Rs 12 no GST -> GST", c.GSTAmount, 0.00m);
    Check("Rs 12 no GST -> base", c.BaseCharge, 12.00m);
    Check("Rs 12 no GST -> total", c.TotalCharge, 12.00m);

    // The worked end-to-end example: provider 5.90 inclusive against a retailer 8.00 exclusive.
    var p = ChargeGstResolver.Split(5.90m, ChargeGstType.Inclusive, 18m);
    var r = ChargeGstResolver.Split(8.00m, ChargeGstType.Exclusive, 18m);
    Check("Rs 8 exclusive -> GST", r.GSTAmount, 1.44m);
    Check("Rs 8 exclusive -> total payable", r.TotalCharge, 9.44m);
    Check("house keeps (provider base less retailer base)", p.BaseCharge - r.BaseCharge, -3.00m);
}

Section("Signs: a charge we PAY is a negative commission, and its GST is input credit");
{
    var s = ChargeGstResolver.Split(-5.90m, ChargeGstType.Inclusive, 18m);
    Check("-5.90 inclusive -> GST stays negative", s.GSTAmount, -0.90m);
    Check("-5.90 inclusive -> base stays negative", s.BaseCharge, -5.00m);
    Check("-5.90 inclusive -> total", s.TotalCharge, -5.90m);

    var e = ChargeGstResolver.Split(-10m, ChargeGstType.Exclusive, 18m);
    Check("-10 exclusive -> GST", e.GSTAmount, -1.80m);
    Check("-10 exclusive -> total", e.TotalCharge, -11.80m);
}

Section("Edges that must not invent tax");
{
    var zeroRate = ChargeGstResolver.Split(5.90m, ChargeGstType.Inclusive, 0m);
    Check("inclusive at 0% -> nothing split out", zeroRate.GSTAmount, 0.00m);
    Check("inclusive at 0% -> base is the whole charge", zeroRate.BaseCharge, 5.90m);

    var zeroCharge = ChargeGstResolver.Split(0m, ChargeGstType.Exclusive, 18m);
    Check("no charge -> no GST", zeroCharge.GSTAmount, 0.00m);

    // A row that says nothing, split directly, must not be read as taxed.
    var dflt = ChargeGstResolver.Split(5.90m, ChargeGstType.Default, 18m);
    Check("DEFAULT split directly -> no GST invented", dflt.GSTAmount, 0.00m);
    CheckText("DEFAULT split directly -> reported as", dflt.GSTType, ChargeGstType.None);

    // A typo in a free-text column falls through; it never becomes "no GST".
    CheckText("'Incl.' normalises to", ChargeGstType.Normalize("Incl."), ChargeGstType.Default);
    CheckText("'inclusive' normalises to", ChargeGstType.Normalize("inclusive"), ChargeGstType.Inclusive);
    CheckText("'No GST' normalises to", ChargeGstType.Normalize("No GST"), ChargeGstType.None);
    CheckText("blank normalises to", ChargeGstType.Normalize(""), ChargeGstType.Default);
}

// ─────────────────────────────────────────────────────────────────────────────────────────────
// 2. The four layers, through the real resolver.
// ─────────────────────────────────────────────────────────────────────────────────────────────


var bands = new FakeBands();
var slabs = new FakeSlabs();
var profiles = new FakeProfiles();
var resolver = new ChargeGstResolver(bands, slabs, profiles, new FakeMappings());

// The house default as it is seeded: inclusive, on the commission, rate from the tax engine.
profiles.Treatment = new CommissionTaxTreatment
{
    Scope = CommissionTaxScope.Global, ScopeLabel = "house default",
    TaxMode = CommissionTaxMode.Inclusive, TaxBase = CommissionTaxBase.Commission,
    GSTRatePercent = 18m, RateFromTaxEngine = true,
};

Section("Layer 1: the band decides, and overrides the profile");
{
    bands.Band = new ProviderCommissionBandMatch
    {
        BandID = 7, FromAmount = 500m, ToAmount = 1000m, SignedCommission = -5.90m,
        ChargeType = ChargeBasis.Flat, GSTType = ChargeGstType.Exclusive, GSTRatePercent = 0m,
    };
    var r = await resolver.ForProviderCharge(3, 10, 0, 700m, -5.90m, DateTime.Today);
    CheckText("band EXCLUSIVE beats profile INCLUSIVE", r.GSTType, ChargeGstType.Exclusive);
    Check("rate falls through to the engine's 18%", r.GSTRatePercent, 18m);
    Check("GST added on top, signed", r.GSTAmount, -1.06m);   // 5.90 x 18% = 1.062 -> 1.06
    Console.WriteLine($"        decided by: {r.DecidedBy}");

    bands.Band.GSTType = ChargeGstType.None;
    var none = await resolver.ForProviderCharge(3, 10, 0, 700m, -5.90m, DateTime.Today);
    CheckText("a band set to NONE is exempt however the profile reads", none.GSTType, ChargeGstType.None);
    Check("...and nothing is split out", none.GSTAmount, 0.00m);

    bands.Band.GSTType = ChargeGstType.Inclusive;
    bands.Band.GSTRatePercent = 12m;
    var pinned = await resolver.ForProviderCharge(3, 10, 0, 700m, -5.90m, DateTime.Today);
    Check("a rate pinned on the band wins over the engine", pinned.GSTRatePercent, 12m);
    Check("5.90 inclusive at 12% -> GST", pinned.GSTAmount, -0.63m); // 5.90 x 12/112 = 0.6321
}

Section("Layer 2: a band that says nothing leaves it to the profile");
{
    bands.Band = new ProviderCommissionBandMatch
    {
        BandID = 7, FromAmount = 500m, ToAmount = 1000m, SignedCommission = -5.90m,
        ChargeType = ChargeBasis.Flat, GSTType = ChargeGstType.Default, GSTRatePercent = 0m,
    };
    var r = await resolver.ForProviderCharge(3, 10, 0, 700m, -5.90m, DateTime.Today);
    CheckText("profile INCLUSIVE applies", r.GSTType, ChargeGstType.Inclusive);
    Check("5.90 inclusive -> GST", r.GSTAmount, -0.90m);
    Check("5.90 inclusive -> base", r.BaseCharge, -5.00m);
    Console.WriteLine($"        decided by: {r.DecidedBy}");
}

Section("Layer 2, GROSS basis: the tax is on the throughput, so the charge is not taxed twice");
{
    profiles.Treatment = new CommissionTaxTreatment
    {
        Scope = CommissionTaxScope.Service, ScopeLabel = "Prepaid Recharge",
        TaxMode = CommissionTaxMode.Inclusive, TaxBase = CommissionTaxBase.Gross,
        GSTRatePercent = 18m, RateFromTaxEngine = true,
    };
    var r = await resolver.ForProviderCharge(1, 4, 0, 1000m, 15m, DateTime.Today);
    CheckText("charge carries no GST of its own under GROSS", r.GSTType, ChargeGstType.None);
    Check("...so nothing is split out of the commission", r.GSTAmount, 0.00m);
    Console.WriteLine($"        decided by: {r.DecidedBy}");

    // ...unless the band says otherwise, which is the more specific statement.
    bands.Band.GSTType = ChargeGstType.Inclusive;
    var overridden = await resolver.ForProviderCharge(1, 4, 0, 1000m, 15m, DateTime.Today);
    CheckText("an explicit band still wins under GROSS", overridden.GSTType, ChargeGstType.Inclusive);
    Check("15.00 inclusive -> GST", overridden.GSTAmount, 2.29m);
}

Section("Layer 4: nothing configured at all is reported, not guessed");
{
    profiles.Treatment = new CommissionTaxTreatment
    {
        Scope = CommissionTaxScope.Global, ScopeLabel = "built-in fallback",
        TaxMode = CommissionTaxMode.Inclusive, TaxBase = CommissionTaxBase.Commission,
        GSTRatePercent = 0m, RateFromTaxEngine = true,
        Warning = "No commission tax profile is configured, so the built-in default was used.",
    };
    bands.Band = null;
    var r = await resolver.ForProviderCharge(3, 10, 0, 700m, -5.90m, DateTime.Today);
    Check("no rate resolved -> nothing split out", r.GSTAmount, 0.00m);
    CheckText("...and the charge stands as quoted", r.BaseCharge.ToString("0.00"), "-5.90");
    bool warned = r.Warning.Contains("0%", StringComparison.OrdinalIgnoreCase);
    if (warned) pass++; else fail++;
    Console.WriteLine($"  {(warned ? "PASS" : "FAIL")}  a 0% rate on a taxable charge is warned about");
    Console.WriteLine($"        warning: {r.Warning}");
}

Section("The downline side: one band, three tiers, one decision");
{
    profiles.Treatment = new CommissionTaxTreatment
    {
        Scope = CommissionTaxScope.Global, ScopeLabel = "house default",
        TaxMode = CommissionTaxMode.Inclusive, TaxBase = CommissionTaxBase.Commission,
        GSTRatePercent = 18m, RateFromTaxEngine = true,
    };
    slabs.Result = new ResolvedCommission
    {
        SlabFound = true, SlabMasterID = 2, SlabDetailID = 44, SlabName = "Retailer Slab",
        FromAmount = 1001m, ToAmount = 5000m,
        GSTType = ChargeGstType.Exclusive, GSTRatePercent = 0m,
        RetailerComm = 10m, DistributorComm = 2m, SuperDistributorComm = 1m,
        RetailerChargeType = ChargeBasis.Flat,
        DistributorChargeType = ChargeBasis.Flat,
        SuperDistributorChargeType = ChargeBasis.Flat,
    };
    var side = await resolver.ForSlabSide(55, 10, 0, 2000m, 10m, 2m, 1m, DateTime.Today);
    CheckText("the band's EXCLUSIVE applies to the retailer", side.Retailer.GSTType, ChargeGstType.Exclusive);
    Check("retailer 10 exclusive -> GST", side.Retailer.GSTAmount, 1.80m);
    Check("retailer 10 exclusive -> payable", side.Retailer.TotalCharge, 11.80m);
    Check("distributor 2 exclusive -> GST", side.Distributor.GSTAmount, 0.36m);
    Check("super-distributor 1 exclusive -> GST", side.SuperDistributor.GSTAmount, 0.18m);
    CheckText("the slab is named for the report", side.SlabName, "Retailer Slab");
    Console.WriteLine($"        decided by: {side.Retailer.DecidedBy}");
}

Console.WriteLine();
Console.WriteLine(new string('=', 80));
Console.WriteLine($"{pass} passed, {fail} failed");
return fail == 0 ? 0 : 1;

// ── The three sources the resolver reads, stood in for ───────────────────────────────────────
// Only these are fakes. The resolver, the decision and the arithmetic under test are the real ones.

// A provider rate card of one band, whose GST decision the test sets.
sealed class FakeBands : IProviderConfigurationAdminCommissionService
{
    public ProviderCommissionBandMatch Band;
    public Task<ProviderCommissionBandMatch> ResolveBand(int p, int s, int o, decimal a) => Task.FromResult(Band);
    public Task<decimal> Resolve(int p, int s, int o, decimal a) => Task.FromResult(Band?.SignedCommission ?? 0m);
    public Task<List<ProviderConfigurationAdminCommissionResponse>> GetAll() => Task.FromResult(new List<ProviderConfigurationAdminCommissionResponse>());
    public Task<ProviderConfigurationAdminCommissionResponse> GetByID(int id) => Task.FromResult<ProviderConfigurationAdminCommissionResponse>(null);
    public Task<List<ProviderConfigurationAdminCommissionResponse>> GetByProviderID(int id) => Task.FromResult(new List<ProviderConfigurationAdminCommissionResponse>());
    public Task<string> Create(CreateProviderConfigurationAdminCommissionRequest r) => Task.FromResult(StatusType.Ok);
    public Task<string> Update(UpdateProviderConfigurationAdminCommissionRequest r) => Task.FromResult(StatusType.Ok);
    public Task<string> Delete(int id) => Task.FromResult(StatusType.Ok);
}

sealed class FakeSlabs : ISlabCommissionResolver
{
    public ResolvedCommission Result = new();
    public Task<ResolvedCommission> Resolve(int u, int s, int o, decimal a) => Task.FromResult(Result);
}

// The profile layer. Returns whatever the test configured, exactly as CommissionTaxResolver would.
sealed class FakeProfiles : ICommissionTaxResolver
{
    public CommissionTaxTreatment Treatment = new();
    public Task<CommissionTaxTreatment> Resolve(int s, int o, int p, int slab, DateTime on) => Task.FromResult(Treatment);
    public CommissionSplit Split(decimal commission, CommissionTaxTreatment t, decimal gross) => new();
}

sealed class FakeMappings : IUserSlabMappingRepository
{
    public Task<List<UserSlabMapping>> GetAll() => Task.FromResult(new List<UserSlabMapping>());
    public Task<List<UserSlabMapping>> GetByUserID(int id) => Task.FromResult(new List<UserSlabMapping>());
    public Task<UserSlabMapping> GetByID(int id) => Task.FromResult<UserSlabMapping>(null);
    public Task<int> Insert(UserSlabMapping e) => Task.FromResult(1);
    public Task<int> Update(UserSlabMapping e) => Task.FromResult(1);
    public Task<int> Delete(int id) => Task.FromResult(1);
}
