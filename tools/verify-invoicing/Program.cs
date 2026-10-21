using FintechApp.Data;
using FintechApp.Repositories;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Services;
using Microsoft.EntityFrameworkCore;

const string Gstin = "27ABCCS3656G1Z8";
const string State = "27";
string cs = @"Server=localhost\SQLEXPRESS;Database=FintechApp;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";
var opts = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs).Options;

int bad = 0;
void Expect(string what, object got, object want)
{
    bool ok = got.ToString() == want.ToString();
    if (!ok) bad++;
    Console.WriteLine($"   {what,-30} got {got,14}   want {want,14}   {(ok ? "ok" : "** MISMATCH **")}");
}

TaxInvoiceService Svc(ApplicationDbContext c)
{
    var cfg = new TaxConfigurationRepository(c);
    return new TaxInvoiceService(c, new TaxEngineService(cfg), new TaxDocumentNumberService(c), cfg);
}
InvoiceGenerationRequest Req(bool dry) => new()
{
    PeriodFrom = new DateTime(2026, 10, 1), PeriodTo = new DateTime(2026, 10, 31),
    CompanyGSTIN = Gstin, SupplierStateCode = State, DryRun = dry, CreatedByUserID = 1,
};

// ── 1. Dry run ───────────────────────────────────────────────────────────────────────
Console.WriteLine("1. DRY RUN - works everything out, writes nothing");
using (var c = new ApplicationDbContext(opts))
{
    var r = await Svc(c).GenerateWalletTopUpInvoices(Req(dry: true));
    foreach (var w in r.Warnings) Console.WriteLine($"   warn: {w[..Math.Min(150, w.Length)]}");
    Expect("documents", r.DocumentsGenerated, 2);
    Expect("gross total", r.GrossTotal, 20000.00m);
    Expect("taxable total", r.TaxableTotal, 16949.16m);   // 2 x 8,474.58
    Expect("rows written", c.TaxInvoiceHeaders.Count(), 0);
    foreach (var d in r.Documents)
        Console.WriteLine($"   -> party {d.PartyUserID} {d.PartyRegistrationStatus,-12} consolidated={d.IsConsolidated,-5} sources={d.SourceCount} gross={d.GrossTotal:N2} taxable={d.TaxableTotal:N2} tax={d.TaxTotal:N2}");
}

// ── 2. Real run ──────────────────────────────────────────────────────────────────────
Console.WriteLine("\n2. REAL RUN");
using (var c = new ApplicationDbContext(opts))
{
    var r = await Svc(c).GenerateWalletTopUpInvoices(Req(dry: false));
    Expect("documents", r.DocumentsGenerated, 2);
    Expect("lines", r.LinesGenerated, 2);
    Expect("annexure rows", r.AnnexureRowsGenerated, 4);
    foreach (var d in r.Documents)
        Console.WriteLine($"   -> {d.DocumentNo,-14} party {d.PartyUserID} {d.PartyRegistrationStatus,-12} consolidated={d.IsConsolidated,-5} sources={d.SourceCount} taxable={d.TaxableTotal:N2} tax={d.TaxTotal:N2} total={d.GrandTotal:N2}");
}

// ── 3. What landed ───────────────────────────────────────────────────────────────────
Console.WriteLine("\n3. WHAT LANDED IN THE TABLES");
using (var c = new ApplicationDbContext(opts))
{
    var regd = c.TaxInvoiceHeaders.Single(h => h.PartyUserID == 2);
    Console.WriteLine($"   registered party 2 -> {regd.DocumentNo}, {regd.SupplyNature}, consolidated={regd.IsConsolidated}");
    Expect("  taxable", regd.TaxableTotal, 8474.58m);
    Expect("  CGST", regd.CGSTTotal, 762.71m);
    Expect("  SGST", regd.SGSTTotal, 762.71m);
    Expect("  IGST", regd.IGSTTotal, 0.00m);
    Expect("  grand total", regd.GrandTotal, 10000.00m);
    Expect("  party GSTIN on doc", regd.PartyGSTIN, "27AAAAA0000A1Z5");

    var b2c = c.TaxInvoiceHeaders.Single(h => h.PartyUserID == 3);
    Console.WriteLine($"   unregistered party 3 -> {b2c.DocumentNo}, consolidated={b2c.IsConsolidated}");
    Expect("  consolidated", b2c.IsConsolidated, true);
    Expect("  grand total", b2c.GrandTotal, 10000.00m);
    Expect("  annexure rows", c.TaxInvoiceAnnexureLines.Count(a => a.TaxInvoiceHeaderID == b2c.TaxInvoiceHeaderID), 3);
    // The annexure must add back to the line it rolls into.
    var sum = c.TaxInvoiceAnnexureLines.Where(a => a.TaxInvoiceHeaderID == b2c.TaxInvoiceHeaderID).Sum(a => a.GrossValue);
    Expect("  annexure gross sums to", sum, 10000.00m);
    Expect("  SAC", c.TaxInvoiceLines.First(l => l.TaxInvoiceHeaderID == b2c.TaxInvoiceHeaderID).SACCode, "998413");
    Expect("  basis", c.TaxInvoiceLines.First(l => l.TaxInvoiceHeaderID == b2c.TaxInvoiceHeaderID).BillingBasis, "GROSS");
}

// ── 4. Idempotency ───────────────────────────────────────────────────────────────────
Console.WriteLine("\n4. RE-RUN - must generate nothing and skip all four sources");
using (var c = new ApplicationDbContext(opts))
{
    var r = await Svc(c).GenerateWalletTopUpInvoices(Req(dry: false));
    Expect("documents", r.DocumentsGenerated, 0);
    Expect("skipped already invoiced", r.SkippedAlreadyInvoiced, 4);
    Expect("headers still", c.TaxInvoiceHeaders.Count(), 2);
}

// Exchange cases run on NOVEMBER so they cannot collide with the October top-up cases: a credit
// to a partner is also a wallet top-up, and keeping the two in one period is what made the design
// issue visible in the first place.
InvoiceGenerationRequest ReqNov(bool dry) => new()
{
    PeriodFrom = new DateTime(2026, 11, 1), PeriodTo = new DateTime(2026, 11, 30),
    CompanyGSTIN = Gstin, SupplierStateCode = State, DryRun = dry, CreatedByUserID = 1,
};

// ── 5. Exchange legs ─────────────────────────────────────────────────────────────────
// A 50,000 swap each way. The party balance nets to zero - that is exactly why this is the
// biggest suspect behind missing turnover - but under GST s.7(1)(a) each leg is a taxable supply,
// so the turnover is 1,00,000.
Console.WriteLine("\n5. EXCHANGE LEGS - detection");
ExchangeLegService Exch(ApplicationDbContext c) => new(c, Svc(c));

// The flag goes on HERE, not in the fixture, so cases 1-4 run with no exchange partner at all.
// That separation is deliberate: a credit to a flagged partner is treated as an exchange supply
// and handed to this register, so leaving the flag on would have pulled party 3's ordinary
// top-ups out of the B2C consolidation case above — which is how the design issue showed up.
using (var c = new ApplicationDbContext(opts))
{
    var p = c.PartyTaxProfiles.Single(x => x.UserID == 3);
    p.IsExchangePartner = true;
    await c.SaveChangesAsync();
    Console.WriteLine("   (party 3 flagged as an exchange partner)");
}
using (var c = new ApplicationDbContext(opts))
{
    var r = await Exch(c).DetectLegs(ReqNov(dry: false));
    foreach (var w in r.Warnings) Console.WriteLine($"   warn: {w[..Math.Min(130, w.Length)]}");
    Expect("outward legs", r.OutwardLegsCreated, 1);
    Expect("inward legs", r.InwardLegsCreated, 1);
}

Console.WriteLine("\n6. EXCHANGE LEGS - re-detection must create nothing");
using (var c = new ApplicationDbContext(opts))
{
    var r = await Exch(c).DetectLegs(ReqNov(dry: false));
    Expect("outward legs", r.OutwardLegsCreated, 0);
    Expect("inward legs", r.InwardLegsCreated, 0);
    Expect("already known", r.LegsAlreadyKnown, 2);
}

Console.WriteLine("\n7. EXCHANGE LEGS - our side gets invoiced, theirs does not");
using (var c = new ApplicationDbContext(opts))
{
    var r = await Exch(c).InvoiceOutwardLegs(ReqNov(dry: false));
    Expect("documents", r.DocumentsGenerated, 1);
    foreach (var d in r.Documents)
        Console.WriteLine($"   -> {d.DocumentNo} to party {d.PartyUserID}: gross {d.GrossTotal:N2}, taxable {d.TaxableTotal:N2}, tax {d.TaxTotal:N2}");
    var outwardLeg = c.ExchangePartnerLegs.Single(l => l.Direction == "OUTWARD");
    var inwardLeg = c.ExchangePartnerLegs.Single(l => l.Direction == "INWARD");
    Expect("outward status", outwardLeg.DocumentStatus, "Invoiced");
    Expect("outward has document", outwardLeg.TaxInvoiceHeaderID.HasValue, true);
    Expect("inward status", inwardLeg.DocumentStatus, "AwaitingDocument");
    Expect("inward has no document", inwardLeg.TaxInvoiceHeaderID.HasValue, false);
    // The document must know it is an exchange supply, or a barter reads back as an ordinary sale.
    var hdr = c.TaxInvoiceHeaders.Single(h => h.TaxInvoiceHeaderID == outwardLeg.TaxInvoiceHeaderID);
    Expect("header links the leg", hdr.ExchangePartnerLegID, outwardLeg.ExchangePartnerLegID);
}

Console.WriteLine("\n8. EXCHANGE REGISTER - net hides it, turnover reports it");
using (var c = new ApplicationDbContext(opts))
{
    var reg = await Exch(c).GetRegister(new DateTime(2026, 11, 1), new DateTime(2026, 11, 30));
    foreach (var row in reg.Rows)
        Console.WriteLine($"   party {row.PartnerUserID} {row.PeriodKey}: outward {row.OutwardAmount:N2}, inward {row.InwardAmount:N2}, "
                        + $"net {row.NetPosition:N2}, TURNOVER {row.TotalTurnover:N2}, awaiting {row.InwardAwaitingDocument}");
    Expect("total outward", reg.TotalOutward, 50000.00m);
    Expect("total inward", reg.TotalInward, 50000.00m);
    Expect("net position", reg.Rows[0].NetPosition, 0.00m);          // looks like nothing happened
    Expect("total turnover", reg.TotalTurnover, 100000.00m);         // what the return needs
    Expect("awaiting document", reg.TotalAwaitingDocument, 1);       // their invoice, not ours
    Expect("awaiting amount", reg.TotalAwaitingAmount, 50000.00m);
}

Console.WriteLine();
Console.WriteLine(bad == 0 ? "ALL CHECKS PASSED" : $"{bad} CHECK(S) FAILED");
return bad == 0 ? 0 : 1;

