using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Jobs;
using FintechApp.Requests.Phase3;
using FintechApp.Requests.Phase6;
using FintechApp.Responses.Phase6;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    public interface IGstMonthService
    {
        /// <summary>Everything about one month's GST, in the order a person deals with it.</summary>
        Task<(GstMonthSummary? Result, string Failure)> Summary(string periodKey);

        /// <summary>
        /// Take the GSTR-2B file from the portal and do everything that follows from it: store
        /// it, match it against the purchase bills, and claim the credit on every bill the
        /// supplier has actually filed.
        /// </summary>
        Task<(Gstr2bAutoResult? Result, string Failure)> Import2bAndClaim(string json, string periodKey, int userId);
    }

    /// <summary>
    /// One month of GST as a single answer.
    ///
    /// The figures already existed, spread over six screens — documents, the purchase register,
    /// the 2B reconciliation, the ITC register, the collection report and three return tabs —
    /// and reading the month meant knowing which of them to open in which order. This puts the
    /// month on one page: what was sold, what was bought, what credit is confirmed, what is left
    /// to pay, and which step is still open. Nothing here is a second calculation: the tax
    /// payable is the GSTR-3B build, the credit is the ITC register, the match is the same
    /// reconciliation the 2B screen runs.
    /// </summary>
    public class GstMonthService : IGstMonthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGstr3bService _gstr3b;
        private readonly IGstr2bImportService _gstr2b;
        private readonly IPurchaseReconService _recon;
        private readonly IItcRegisterService _itc;
        private readonly MonthlyInvoiceJob _monthly;
        private readonly Logger _logger = new();

        public GstMonthService(ApplicationDbContext context, IGstr3bService gstr3b, IGstr2bImportService gstr2b,
            IPurchaseReconService recon, IItcRegisterService itc, MonthlyInvoiceJob monthly)
        {
            _context = context;
            _gstr3b = gstr3b;
            _gstr2b = gstr2b;
            _recon = recon;
            _itc = itc;
            _monthly = monthly;
        }

        public async Task<(GstMonthSummary? Result, string Failure)> Summary(string periodKey)
        {
            string period = (periodKey ?? string.Empty).Trim();
            if (period.Length != 6 || !int.TryParse(period[..4], out int y) || !int.TryParse(period[4..], out int m) || m is < 1 or > 12)
                return (null, "Choose a month.");

            var profile = await _context.InvoiceSettings.AsNoTracking()
                .OrderBy(x => x.InvoiceSettingID).FirstOrDefaultAsync();
            string gstin = (profile?.GSTIN ?? string.Empty).Trim().ToUpperInvariant();
            if (gstin.Length != 15)
                return (null, "The company GSTIN is not set. Fill it in on Invoice Settings and this page will work.");

            var from = new DateTime(y, m, 1);
            var to = from.AddMonths(1);
            bool monthOpen = to > DateTime.Today;

            var (g3, failure) = await _gstr3b.Build(new GstReturnRequest { CompanyGSTIN = gstin, PeriodKey = period });
            if (g3 == null) return (null, failure);

            var s = new GstMonthSummary
            {
                CompanyGSTIN = gstin,
                CompanyName = profile?.CompanyName ?? string.Empty,
                PeriodKey = period,
                PeriodLabel = from.ToString("MMMM yyyy"),
                MonthStillOpen = monthOpen,
                Gstr1DueDate = to.AddDays(10),      // the 11th of the following month
                Gstr3bDueDate = to.AddDays(19),     // the 20th
                Warnings = g3.Warnings,
            };

            // The portal sign-in name is a reminder on the page, nothing more. The password is
            // never read here or anywhere else.
            s.PortalUsername = await _context.SettingMasters.AsNoTracking()
                .Where(x => x.Key == "GstPortalUsername" && !x.IsDeleted)
                .Select(x => x.Value).FirstOrDefaultAsync() ?? string.Empty;

            // ── 1. Sales ────────────────────────────────────────────────────────────────
            var sales = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Sale && h.PeriodKey == period
                         && h.CompanyGSTIN == gstin && h.Status == TaxDocumentStatus.Issued
                         && h.DocumentType != TaxDocumentType.SelfInvoice)
                .Select(h => new { h.DocumentType, h.GrandTotal })
                .ToListAsync();
            s.SaleInvoiceCount = sales.Count;
            s.SaleInvoiceValue = sales.Sum(x => x.DocumentType == TaxDocumentType.CreditNote ? -x.GrandTotal : x.GrandTotal);
            s.SaleTaxableValue = g3.Gstr1TaxableValue;
            s.OutputIGST = g3.OutputIGST; s.OutputCGST = g3.OutputCGST; s.OutputSGST = g3.OutputSGST;
            s.OutputTax = g3.OutputIGST + g3.OutputCGST + g3.OutputSGST + g3.OutputCess;

            // What has happened but carries no invoice yet. A dry run of the generator — the
            // same code that issues — so the count here is exactly what pressing Generate gives.
            try
            {
                var pending = await _monthly.Generate(period, dryRun: true, createdByUserID: 0);
                s.PendingInvoiceCount = pending.Refused ? 0 : pending.DocumentsGenerated;
                s.PendingInvoiceTaxable = pending.Refused ? 0m : pending.TaxableTotal;
                s.PendingInvoiceTax = pending.Refused ? 0m : pending.TaxTotal;
                if (pending.Refused && pending.Blockers.Count > 0) s.PendingInvoiceBlocker = pending.RefusalReason;
            }
            catch (Exception ex) { _logger.Write(ex); }

            // ── 2. Purchases and the credit on them ─────────────────────────────────────
            var purchases = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Purchase && h.PeriodKey == period)
                .OrderBy(h => h.DocumentDate).ThenBy(h => h.TaxInvoiceHeaderID)
                .ToListAsync();
            var purchaseIds = purchases.Select(p => p.TaxInvoiceHeaderID).ToList();
            var entries = (await _context.ItcRegisterEntries.AsNoTracking()
                    .Where(e => purchaseIds.Contains(e.TaxInvoiceHeaderID) && !e.IsDeleted).ToListAsync())
                .GroupBy(e => e.TaxInvoiceHeaderID).ToDictionary(g => g.Key, g => g.First());

            s.PurchaseCount = purchases.Count;
            s.PurchaseTaxableValue = purchases.Sum(p => p.TaxableTotal);
            s.PurchaseTax = purchases.Sum(p => p.CGSTTotal + p.SGSTTotal + p.IGSTTotal + p.CessTotal);

            var waiting = GstSetOff.Heads.Zero;
            foreach (var p in purchases)
            {
                entries.TryGetValue(p.TaxInvoiceHeaderID, out var e);
                decimal tax = p.CGSTTotal + p.SGSTTotal + p.IGSTTotal + p.CessTotal;
                string status = e?.Status ?? string.Empty;
                // Where a bill has no register entry yet (captured before the register was kept
                // up automatically), its whole tax is treated as credit waiting to be confirmed.
                decimal eligible = e?.EligibleITC ?? tax;

                if (status == ItcStatus.Claimed) s.CreditConfirmed += e!.EligibleITC;
                else if (status != ItcStatus.Reversed && status != ItcStatus.Ineligible && eligible > 0m)
                {
                    s.CreditWaiting += eligible;
                    waiting += e == null
                        ? new GstSetOff.Heads(p.IGSTTotal, p.CGSTTotal, p.SGSTTotal, p.CessTotal)
                        : new GstSetOff.Heads(e.IGST, e.CGST, e.SGST, e.Cess);
                }

                s.Purchases.Add(new GstMonthPurchaseRow
                {
                    TaxInvoiceHeaderID = p.TaxInvoiceHeaderID,
                    DocumentNo = p.DocumentNo,
                    DocumentDate = p.DocumentDate,
                    SupplierName = p.PartyName,
                    SupplierGSTIN = p.PartyGSTIN,
                    TaxableValue = p.TaxableTotal,
                    Tax = tax,
                    Total = p.GrandTotal,
                    Credit = eligible,
                    CreditStatus = status == ItcStatus.Claimed ? "Confirmed"
                                 : status == ItcStatus.Reversed ? "Reversed"
                                 : status == ItcStatus.Ineligible ? "No credit"
                                 : (e?.ReconBucket ?? string.Empty) == ReconBucket.Mismatch ? "Tax differs from 2B"
                                 : (e?.ReconBucket ?? string.Empty) == ReconBucket.MissingIn2B ? "Supplier has not filed"
                                 : "Waiting for 2B",
                });
            }

            // ── 3. The 2B file ──────────────────────────────────────────────────────────
            var import = await _context.Gstr2bImports.AsNoTracking()
                .Where(i => i.CompanyGSTIN == gstin && i.PeriodKey == period
                         && i.Status == Gstr2bImportStatus.Active && !i.IsDeleted)
                .OrderByDescending(i => i.Gstr2bImportID).FirstOrDefaultAsync();
            s.TwoBImported = import != null;
            s.TwoBImportedOn = import?.DateTime;
            s.TwoBLineCount = import?.LineCount ?? 0;
            s.TwoBTax = import?.TotalTax ?? 0m;

            if (import != null)
            {
                var recon = await _recon.Reconcile(new ReconRequest { CompanyGSTIN = gstin, PeriodKey = period }, commit: false);
                foreach (var b in recon.Summary)
                {
                    if (b.Bucket == ReconBucket.Matched) s.TwoBMatched = b.Count;
                    else if (b.Bucket == ReconBucket.MissingIn2B) s.TwoBMissingIn2B = b.Count;
                    else if (b.Bucket == ReconBucket.MissingInBooks) s.TwoBMissingInBooks = b.Count;
                    else if (b.Bucket == ReconBucket.Mismatch) s.TwoBMismatch = b.Count;
                }
                s.TwoBMissingBills = recon.Rows.Where(r => r.Bucket == ReconBucket.MissingInBooks)
                    .Select(r => new GstMonthMissingBill
                    {
                        SupplierName = r.SupplierName, SupplierGSTIN = r.SupplierGSTIN,
                        DocumentNo = r.DocumentNo, DocumentDate = r.DocumentDate, Tax = r.Portal2BTax,
                    }).ToList();
            }

            // ── 4. What is left to pay ──────────────────────────────────────────────────
            s.OpeningCredit = g3.OpeningCreditIGST + g3.OpeningCreditCGST + g3.OpeningCreditSGST + g3.OpeningCreditCess;
            s.CreditUsedThisMonth = g3.NetItcIGST + g3.NetItcCGST + g3.NetItcSGST + g3.NetItcCess;
            s.PayableIGST = g3.PayableIGST; s.PayableCGST = g3.PayableCGST; s.PayableSGST = g3.PayableSGST;
            s.TotalPayable = g3.TotalPayable;
            s.ClosingCredit = g3.ClosingCreditIGST + g3.ClosingCreditCGST + g3.ClosingCreditSGST + g3.ClosingCreditCess;

            // The same sum again as it WOULD stand once the waiting credit is confirmed. Shown
            // beside the real figure, never instead of it: a credit the supplier has not filed
            // is not ours to use yet, but a page that shows the full tax as payable next to
            // lakhs of unconfirmed credit reads as a mistake unless it says what happens next.
            var after = GstSetOff.Apply(
                new GstSetOff.Heads(g3.OutputIGST, g3.OutputCGST, g3.OutputSGST, g3.OutputCess),
                new GstSetOff.Heads(g3.OpeningCreditIGST + g3.NetItcIGST, g3.OpeningCreditCGST + g3.NetItcCGST,
                                    g3.OpeningCreditSGST + g3.NetItcSGST, g3.OpeningCreditCess + g3.NetItcCess) + waiting);
            s.PayableIfCreditConfirmed = Math.Round(after.Payable.Total, 2);
            s.ClosingCreditIfConfirmed = Math.Round(after.CreditLeft.Total, 2);

            // ── 5. Filing ───────────────────────────────────────────────────────────────
            var filings = await _context.GstReturnFilings.AsNoTracking()
                .Where(f => !f.IsDeleted && f.CompanyGSTIN == gstin && f.PeriodKey == period
                         && f.Status == GstFilingStatus.Filed)
                .Select(f => new { f.ReturnType, f.FiledOn, f.AcknowledgementNo })
                .ToListAsync();
            var f1 = filings.FirstOrDefault(f => f.ReturnType == GstReturnType.Gstr1);
            var f3 = filings.FirstOrDefault(f => f.ReturnType == GstReturnType.Gstr3b);
            s.Gstr1Filed = f1 != null; s.Gstr1FiledOn = f1?.FiledOn; s.Gstr1Ack = f1?.AcknowledgementNo ?? string.Empty;
            s.Gstr3bFiled = f3 != null; s.Gstr3bFiledOn = f3?.FiledOn; s.Gstr3bAck = f3?.AcknowledgementNo ?? string.Empty;
            s.AgreesWithGstr1 = g3.AgreesWithGstr1;

            return (s, string.Empty);
        }

        public async Task<(Gstr2bAutoResult? Result, string Failure)> Import2bAndClaim(string json, string periodKey, int userId)
        {
            var profile = await _context.InvoiceSettings.AsNoTracking()
                .OrderBy(x => x.InvoiceSettingID).FirstOrDefaultAsync();
            string gstin = (profile?.GSTIN ?? string.Empty).Trim().ToUpperInvariant();
            if (gstin.Length != 15) return (null, "The company GSTIN is not set on Invoice Settings.");

            // The file says which registration and month it belongs to. Read both BEFORE storing
            // anything: last month's file, or another company's, dropped onto this month would
            // otherwise mark this month's bills as "supplier has not filed".
            try
            {
                var root = Newtonsoft.Json.Linq.JObject.Parse(json ?? string.Empty);
                var data = (root["data"] as Newtonsoft.Json.Linq.JObject) ?? root;
                string fileGstin = ((string?)data["gstin"] ?? string.Empty).Trim().ToUpperInvariant();
                string rtn = ((string?)data["rtnprd"] ?? string.Empty).Trim();
                string filePeriod = rtn.Length == 6 ? rtn.Substring(2, 4) + rtn.Substring(0, 2) : string.Empty;

                if (fileGstin.Length == 15 && fileGstin != gstin)
                    return (null, $"This file is for GSTIN {fileGstin}, not {gstin}. Download the GSTR-2B of your own registration.");
                if (filePeriod.Length == 6 && !string.IsNullOrWhiteSpace(periodKey) && filePeriod != periodKey.Trim())
                    return (null, $"This file is the GSTR-2B for {PeriodName(filePeriod)}, but the month open on the page is {PeriodName(periodKey.Trim())}. Choose that month, or download the right file.");
                if (string.IsNullOrWhiteSpace(periodKey)) periodKey = filePeriod;
            }
            catch
            {
                return (null, "That file could not be read. Upload the GSTR-2B JSON exactly as downloaded from the GST portal (Returns → GSTR-2B → Download → JSON).");
            }

            var (imported, importFailure) = await _gstr2b.Import(new Gstr2bImportRequest
            {
                Json = json!, CompanyGSTIN = gstin, PeriodKey = periodKey ?? string.Empty, ImportedByUserID = userId,
            });
            if (imported == null) return (null, importFailure);

            string period = imported.PeriodKey;
            var result = new Gstr2bAutoResult
            {
                PeriodKey = period,
                LinesInFile = imported.LinesImported,
                TaxInFile = imported.TotalTax,
                Warnings = imported.Warnings,
            };

            // The register first, so every bill of the month has an entry for the match to land on.
            await _itc.BuildRegister(new ItcBuildRequest { PeriodKey = period, CreatedByUserID = userId });

            var recon = await _recon.Reconcile(
                new ReconRequest { CompanyGSTIN = gstin, PeriodKey = period, CreatedByUserID = userId }, commit: true);
            foreach (var b in recon.Summary)
            {
                if (b.Bucket == ReconBucket.Matched) result.Matched = b.Count;
                else if (b.Bucket == ReconBucket.MissingIn2B) result.MissingIn2B = b.Count;
                else if (b.Bucket == ReconBucket.MissingInBooks) result.MissingInBooks = b.Count;
                else if (b.Bucket == ReconBucket.Mismatch) result.Mismatch = b.Count;
            }

            // Claim what matched, and only that. A bill the supplier has not filed, or filed
            // with different tax, stays unclaimed and is named on the page.
            var toClaim = await _context.ItcRegisterEntries.AsNoTracking()
                .Where(e => !e.IsDeleted && e.PeriodKey == period && e.Status == ItcStatus.Eligible
                         && e.ReconBucket == ReconBucket.Matched && e.EligibleITC > 0m)
                .Select(e => new { e.ItcRegisterEntryID, e.EligibleITC }).ToListAsync();

            if (toClaim.Count > 0)
            {
                var (claimed, _) = await _itc.Claim(new ItcClaimRequest
                {
                    EntryIDs = toClaim.Select(x => x.ItcRegisterEntryID).ToList(),
                    ClaimPeriod = period,
                    CreatedByUserID = userId,
                });
                result.CreditsClaimed = claimed;
                result.CreditClaimedValue = toClaim.Sum(x => x.EligibleITC);
            }

            return (result, string.Empty);
        }

        private static string PeriodName(string period) =>
            period.Length == 6 && int.TryParse(period[..4], out int y) && int.TryParse(period[4..], out int m) && m is >= 1 and <= 12
                ? new DateTime(y, m, 1).ToString("MMMM yyyy") : period;
    }
}

namespace FintechApp.Responses.Phase6
{
    public class GstMonthSummary
    {
        public string CompanyGSTIN { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        public bool MonthStillOpen { get; set; }
        public string PortalUsername { get; set; } = string.Empty;
        public DateTime Gstr1DueDate { get; set; }
        public DateTime Gstr3bDueDate { get; set; }

        // 1. Sales
        public int SaleInvoiceCount { get; set; }
        public decimal SaleInvoiceValue { get; set; }
        public decimal SaleTaxableValue { get; set; }
        public decimal OutputIGST { get; set; }
        public decimal OutputCGST { get; set; }
        public decimal OutputSGST { get; set; }
        public decimal OutputTax { get; set; }
        public int PendingInvoiceCount { get; set; }
        public decimal PendingInvoiceTaxable { get; set; }
        public decimal PendingInvoiceTax { get; set; }
        public string PendingInvoiceBlocker { get; set; } = string.Empty;

        // 2. Purchases
        public int PurchaseCount { get; set; }
        public decimal PurchaseTaxableValue { get; set; }
        public decimal PurchaseTax { get; set; }
        public decimal CreditConfirmed { get; set; }
        public decimal CreditWaiting { get; set; }
        public List<GstMonthPurchaseRow> Purchases { get; set; } = new();

        // 3. GSTR-2B
        public bool TwoBImported { get; set; }
        public DateTime? TwoBImportedOn { get; set; }
        public int TwoBLineCount { get; set; }
        public decimal TwoBTax { get; set; }
        public int TwoBMatched { get; set; }
        public int TwoBMissingIn2B { get; set; }
        public int TwoBMissingInBooks { get; set; }
        public int TwoBMismatch { get; set; }
        public List<GstMonthMissingBill> TwoBMissingBills { get; set; } = new();

        // 4. Payable
        public decimal OpeningCredit { get; set; }
        public decimal CreditUsedThisMonth { get; set; }
        public decimal PayableIGST { get; set; }
        public decimal PayableCGST { get; set; }
        public decimal PayableSGST { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal ClosingCredit { get; set; }
        public decimal PayableIfCreditConfirmed { get; set; }
        public decimal ClosingCreditIfConfirmed { get; set; }

        // 5. Filing
        public bool Gstr1Filed { get; set; }
        public DateTime? Gstr1FiledOn { get; set; }
        public string Gstr1Ack { get; set; } = string.Empty;
        public bool Gstr3bFiled { get; set; }
        public DateTime? Gstr3bFiledOn { get; set; }
        public string Gstr3bAck { get; set; } = string.Empty;
        public bool AgreesWithGstr1 { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    public class GstMonthPurchaseRow
    {
        public int TaxInvoiceHeaderID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierGSTIN { get; set; } = string.Empty;
        public decimal TaxableValue { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal Credit { get; set; }
        public string CreditStatus { get; set; } = string.Empty;
    }

    public class GstMonthMissingBill
    {
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierGSTIN { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; }
        public decimal Tax { get; set; }
    }

    public class Gstr2bAutoResult
    {
        public string PeriodKey { get; set; } = string.Empty;
        public int LinesInFile { get; set; }
        public decimal TaxInFile { get; set; }
        public int Matched { get; set; }
        public int MissingIn2B { get; set; }
        public int MissingInBooks { get; set; }
        public int Mismatch { get; set; }
        public int CreditsClaimed { get; set; }
        public decimal CreditClaimedValue { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}

namespace FintechApp.Requests.Phase6
{
    public class GstMonthRequest
    {
        public string PeriodKey { get; set; } = string.Empty;
    }

    public class Gstr2bAutoRequest
    {
        public string Json { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
    }
}
