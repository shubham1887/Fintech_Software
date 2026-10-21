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
    // Compare decimals numerically: 0m and 0.00m are the same money but different strings, and a
    // test that fails on trailing zeros teaches people to ignore it.
    bool ok = got is decimal g && want is decimal w
        ? decimal.Compare(g, w) == 0
        : got.ToString() == want.ToString();
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

// ── 9. Credit notes ──────────────────────────────────────────────────────────────────
Console.WriteLine("\n9. CREDIT NOTE - partial, cumulative cap, and the 30-November window");
int regdHeaderId;
using (var c = new ApplicationDbContext(opts))
    regdHeaderId = c.TaxInvoiceHeaders.First(h => h.PartyUserID == 2 && h.DocumentType == "TaxInvoice").TaxInvoiceHeaderID;

CreditNoteService Cn(ApplicationDbContext c) => new(c, new TaxDocumentNumberService(c));
CreditNoteRequest CnReq(decimal amount, DateTime? on = null) => new()
{
    OriginalInvoiceHeaderID = regdHeaderId, GrossAmount = amount, Reason = "ZZPROBE reversal",
    NoteDate = on, CompanyGSTIN = Gstin, SupplierStateCode = State, CreatedByUserID = 1,
};

// The supply was 05-Oct-2026, so FY 2026-27, so the window closes 30-Nov-2027.
Expect("window close date", CreditNoteService.WindowCloseFor(new DateTime(2026, 10, 5)), new DateTime(2027, 11, 30));
Expect("window, 31-Mar case", CreditNoteService.WindowCloseFor(new DateTime(2026, 3, 31)), new DateTime(2026, 11, 30));

using (var c = new ApplicationDbContext(opts))
{
    // 2,000 of a 10,000 invoice: a fifth, so the tax is a fifth too.
    var (doc, err) = await Cn(c).Issue(CnReq(2000m, new DateTime(2026, 10, 20)));
    if (err.Length > 0) Console.WriteLine($"   note: {err[..Math.Min(120, err.Length)]}");
    Expect("partial note raised", doc != null, true);
    Console.WriteLine($"   -> {doc?.DocumentNo} gross {doc?.GrossTotal:N2} taxable {doc?.TaxableTotal:N2} tax {doc?.TaxTotal:N2}");
    Expect("  gross", doc!.GrossTotal, 2000.00m);
    Expect("  taxable", doc.TaxableTotal, 1694.92m);
    Expect("  tax", doc.TaxTotal, 305.08m);
    var saved = c.TaxInvoiceHeaders.Single(h => h.TaxInvoiceHeaderID == doc.TaxInvoiceHeaderID);
    Expect("  links the original", saved.OriginalInvoiceHeaderID, regdHeaderId);
    Expect("  window stored", saved.NoteTimeLimitDate, new DateTime(2027, 11, 30));
    Expect("  own series", saved.DocumentNo.StartsWith("CN/"), true);
}

using (var c = new ApplicationDbContext(opts))
{
    // Asking for more than remains must be refused, not silently capped.
    var (doc, err) = await Cn(c).Issue(CnReq(9000m, new DateTime(2026, 10, 21)));
    Expect("over-credit refused", doc == null, true);
    Console.WriteLine($"   refusal: {err[..Math.Min(110, err.Length)]}");
}

using (var c = new ApplicationDbContext(opts))
{
    var (doc, _) = await Cn(c).Issue(CnReq(0m, new DateTime(2026, 10, 22)));   // 0 = whatever is left
    Expect("remainder credited", doc!.GrossTotal, 8000.00m);
    var (again, err2) = await Cn(c).Issue(CnReq(0m, new DateTime(2026, 10, 23)));
    Expect("fully credited, refused", again == null, true);
    Console.WriteLine($"   refusal: {err2[..Math.Min(110, err2.Length)]}");
}

using (var c = new ApplicationDbContext(opts))
{
    // Past the window: the note still issues, but WITHOUT tax - the GST is gone.
    var b2c = c.TaxInvoiceHeaders.First(h => h.PartyUserID == 3 && h.DocumentType == "TaxInvoice");
    var (doc, err) = await Cn(c).Issue(new CreditNoteRequest
    {
        OriginalInvoiceHeaderID = b2c.TaxInvoiceHeaderID, GrossAmount = 1000m,
        Reason = "ZZPROBE late reversal", NoteDate = new DateTime(2027, 12, 1),
        CompanyGSTIN = Gstin, SupplierStateCode = State, CreatedByUserID = 1,
    });
    Expect("late note still issued", doc != null, true);
    Expect("  but carries no tax", doc!.TaxTotal, 0.00m);
    Expect("  warning given", err.Contains("can no longer be recovered"), true);
}

// ── 10. e-invoice queue ──────────────────────────────────────────────────────────────
Console.WriteLine("\n10. E-INVOICE QUEUE - deadline, idempotency, and an honest failure");
EInvoiceService Einv(ApplicationDbContext c) => new(c, new NotConfiguredEInvoiceGateway());
using (var c = new ApplicationDbContext(opts))
{
    var (queued, msg) = await Einv(c).Enqueue(regdHeaderId);
    Expect("queued", queued, true);
    Console.WriteLine($"   {msg}");
    var log = c.EInvoiceLogs.Single(l => l.TaxInvoiceHeaderID == regdHeaderId);
    // 05-Oct-2026 + 30 days.
    Expect("  deadline", log.ReportingDeadline, new DateTime(2026, 11, 4));
    Expect("  status", log.Status, "Pending");

    var (again, _) = await Einv(c).Enqueue(regdHeaderId);
    Expect("re-queue refused", again, false);
}

using (var c = new ApplicationDbContext(opts))
{
    int n = await Einv(c).EnqueuePeriod(new DateTime(2026, 10, 1), new DateTime(2027, 12, 31));
    Console.WriteLine($"   period sweep queued {n} more document(s)");
    // Credit notes are reported too; bills of supply are not.
    Expect("nothing left unqueued", c.TaxInvoiceHeaders.Count(h =>
        h.Direction == "SALE" && h.Status == "Issued"
        && !c.EInvoiceLogs.Any(l => l.TaxInvoiceHeaderID == h.TaxInvoiceHeaderID)), 0);
}

using (var c = new ApplicationDbContext(opts))
{
    var r = await Einv(c).ProcessQueue(50);
    Console.WriteLine($"   attempted {r.Attempted}, registered {r.Registered}, retryable {r.FailedRetryable}, "
                    + $"permanent {r.FailedPermanent}, past deadline {r.PastDeadline}");
    // With no IRP configured nothing can register. What matters is that the documents stay
    // Pending and keep showing on the alert, rather than being marked done.
    Expect("nothing registered", r.Registered, 0);
    // Retryable, not permanent: the documents stay Pending so the next pass picks them up and the
    // deadline alert keeps showing them. Marking them done would be the dangerous outcome.
    Expect("all retryable", r.FailedRetryable, r.Attempted);
    Expect("none permanent", r.FailedPermanent, 0);
    // Nothing is past its deadline yet on these dates, which is the correct answer today.
    Expect("none past deadline yet", r.PastDeadline, 0);
    foreach (var m in r.Messages) Console.WriteLine($"   msg: {m[..Math.Min(120, m.Length)]}");
}

using (var c = new ApplicationDbContext(opts))
{
    var alerts = await Einv(c).GetDeadlineAlerts(new DateTime(2026, 11, 1), 30);
    Console.WriteLine($"   {alerts.Count} alert(s) as of 01-Nov-2026:");
    foreach (var a in alerts)
        Console.WriteLine($"     {a.DocumentNo} due {a.ReportingDeadline:dd-MMM-yyyy}, {a.DaysLeft} day(s) left, passed={a.DeadlinePassed}");
    Expect("alerts raised", alerts.Count > 0, true);
    Expect("none passed on 01-Nov", alerts.Count(a => a.DeadlinePassed), 0);
}

using (var c = new ApplicationDbContext(opts))
{
    // Seen from a year later, every one of those deadlines has gone. This is the state that costs
    // money: no IRN can be obtained any more, and the only route left is a credit note plus a
    // fresh invoice.
    var alerts = await Einv(c).GetDeadlineAlerts(new DateTime(2027, 11, 1), 0);
    Expect("all passed a year later", alerts.All(a => a.DeadlinePassed), true);
    Console.WriteLine($"   as of 01-Nov-2027: {alerts.Count} document(s) past the IRP deadline");
}

// ── 11. Self-invoice ─────────────────────────────────────────────────────────────────
Console.WriteLine("\n11. SELF-INVOICE - refused without a reverse-charge rule, issued with one");
using (var c = new ApplicationDbContext(opts))
{
    int plainService = c.ServiceMasters.First(s => s.ServiceCategory == "Money Transfer").ServiceID;
    var (doc, err) = await Svc(c).IssueSelfInvoice(new SelfInvoiceRequest
    {
        SupplierUserID = 3, ServiceID = plainService, Amount = 5000m,
        CompanyGSTIN = Gstin, SupplierStateCode = State, CreatedByUserID = 1,
        DocumentDate = new DateTime(2026, 10, 25),
    });
    Expect("no RCM rule -> refused", doc == null, true);
    Console.WriteLine($"   refusal: {err[..Math.Min(130, err.Length)]}");

    int rcService = c.ServiceMasters.First(s => s.ServiceCategory == "Verification Services").ServiceID;
    var (doc2, err2) = await Svc(c).IssueSelfInvoice(new SelfInvoiceRequest
    {
        SupplierUserID = 3, ServiceID = rcService, Amount = 5000m,
        Description = "ZZPROBE reverse-charge supply",
        CompanyGSTIN = Gstin, SupplierStateCode = State, CreatedByUserID = 1,
        DocumentDate = new DateTime(2026, 10, 25),
    });
    Expect("RCM rule -> issued", doc2 != null, true);
    if (doc2 != null)
    {
        Console.WriteLine($"   -> {doc2.DocumentNo} taxable {doc2.TaxableTotal:N2} tax {doc2.TaxTotal:N2}");
        var hdr = c.TaxInvoiceHeaders.Single(h => h.TaxInvoiceHeaderID == doc2.TaxInvoiceHeaderID);
        Expect("  document type", hdr.DocumentType, "SelfInvoice");
        Expect("  own series", hdr.DocumentNo.StartsWith("SV/"), true);
        // Tax is ADDED on top: an unregistered supplier charged none to begin with.
        Expect("  taxable", doc2.TaxableTotal, 5000.00m);
        Expect("  tax at 18%", doc2.TaxTotal, 900.00m);
        var line = c.TaxInvoiceLines.Single(l => l.TaxInvoiceHeaderID == hdr.TaxInvoiceHeaderID);
        Expect("  reverse charge on line", line.ReverseCharge, true);
        Expect("  ITC eligible", line.ITCEligible, true);
    }
    else Console.WriteLine($"   unexpected refusal: {err2}");
}

Console.WriteLine();
Console.WriteLine(bad == 0 ? "ALL CHECKS PASSED" : $"{bad} CHECK(S) FAILED");
return bad == 0 ? 0 : 1;

