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

Console.WriteLine();
Console.WriteLine(bad == 0 ? "ALL CHECKS PASSED" : $"{bad} CHECK(S) FAILED");
return bad == 0 ? 0 : 1;

