using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses.TaxInvoice;
using FintechApp.StaticModels;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Jobs
{
    /// <summary>
    /// Issues the month's own sale documents, without anybody remembering to.
    ///
    /// Why this is a job and not only a button: a GST month closes whether or not the operator
    /// opens a screen. On this install nothing had ever been generated, so there were no sale
    /// invoices at all — not because generation was broken, but because the only way to run it was
    /// to know it existed. That is a defect in a product sold as a white-label install, where the
    /// person running it is not the person who built it.
    ///
    /// The button and the schedule run the SAME method, which is the point of it living here: the
    /// refusal below has to behave identically whether a person or a cron started it, and two
    /// copies of that rule would eventually disagree.
    ///
    /// It is safe to run repeatedly. Generation is idempotent at the source: every fund
    /// transaction and every transaction master already carried by a document is skipped
    /// (<c>SkippedAlreadyInvoiced</c>), so a second run on the same month issues nothing and an
    /// interrupted run simply finishes the rest.
    ///
    /// It refuses rather than guesses. If a party due a document has no place of supply the run
    /// STOPS and names them — it does not issue. A document keeps the state it was stamped with,
    /// the portal rejects a blank one, and withdrawing it means a cancellation against a numbering
    /// series that has to stay demonstrable. Issuing a bad document automatically, every month, on
    /// a schedule, is far worse than issuing none and saying so.
    /// </summary>
    public class MonthlyInvoiceJob
    {
        public const string JobId = "sale-invoices-monthly";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyInvoiceJob> _log;

        public MonthlyInvoiceJob(IServiceScopeFactory scopeFactory, ILogger<MonthlyInvoiceJob> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;
        }

        /// <summary>The scheduled entry point: the month that has just ended.</summary>
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public async Task Run()
        {
            var result = await Generate(null, dryRun: false, createdByUserID: 0);

            if (result.Refused)
                _log.LogWarning(
                    "Month-end invoicing for {Period}: NOTHING ISSUED. {Reason} {Details}",
                    result.PeriodKey, result.RefusalReason, string.Join(" | ", result.Blockers));
            else if (result.DocumentsGenerated == 0)
                _log.LogInformation(
                    "Month-end invoicing for {Period}: nothing left to issue — every source is already carried "
                  + "by a document.", result.PeriodKey);
            else
                _log.LogInformation(
                    "Month-end invoicing for {Period}: issued {Docs} document(s), taxable {Taxable:N2}, "
                  + "tax {Tax:N2}. Already invoiced and skipped: {Skipped}.",
                    result.PeriodKey, result.DocumentsGenerated, result.TaxableTotal, result.TaxTotal,
                    result.SkippedAlreadyInvoiced);
        }

        /// <summary>
        /// Generate (or preview) one month. <paramref name="periodKey"/> is yyyyMM; null means the
        /// month that has just ended, which is what the schedule wants.
        ///
        /// A dry run always happens first, even when issuing, because the preview is the only
        /// thing that can say WHICH parties actually receive a document this month — and that is
        /// the question that decides whether it is safe to issue at all.
        /// </summary>
        public Task<MonthlyInvoiceResult> Generate(string? periodKey, bool dryRun, int createdByUserID)
            => Generate(periodKey, dryRun, createdByUserID, consolidation: null, saveAsDefault: false);

        public async Task<MonthlyInvoiceResult> Generate(
            string? periodKey, bool dryRun, int createdByUserID, string? consolidation, bool saveAsDefault)
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var invoices = sp.GetRequiredService<ITaxInvoiceService>();

            // The month that just ended, unless one was named. Running on the 2nd for "this month"
            // would invoice a month that is still happening.
            DateTime anchor = DateTime.Today.AddMonths(-1);
            if (!string.IsNullOrWhiteSpace(periodKey))
            {
                if (periodKey.Length != 6
                    || !int.TryParse(periodKey[..4], out int y)
                    || !int.TryParse(periodKey[4..], out int m)
                    || m is < 1 or > 12)
                {
                    return Refuse(periodKey, "That is not a yyyyMM period.");
                }
                anchor = new DateTime(y, m, 1);
            }

            var from = new DateTime(anchor.Year, anchor.Month, 1);
            var to = from.AddMonths(1).AddDays(-1);
            string period = $"{from:yyyyMM}";

            // The month still in progress is invoiced up to YESTERDAY, and only when somebody
            // asks for it by name — the unattended run never names a period, so it always takes
            // the month that has ended. What arrives later in the month goes on a further
            // document at month end; nothing is billed twice, because generation skips every
            // source a document already carries.
            bool partialMonth = false;
            if (to >= DateTime.Today)
            {
                if (from > DateTime.Today)
                    return Refuse(period, "That month has not started yet.");
                to = DateTime.Today.AddDays(-1);
                if (to < from)
                    return Refuse(period, "This month has only just started — there is nothing to invoice until tomorrow.");
                partialMonth = true;
            }

            // The registration raising the documents, from the company profile rather than from
            // configuration: it is already filled in on every install that can issue at all, and a
            // second copy in appsettings is one more thing to get wrong per client.
            var profile = await db.InvoiceSettings.AsNoTracking()
                .OrderBy(x => x.InvoiceSettingID)
                .FirstOrDefaultAsync();

            string gstin = (profile?.GSTIN ?? string.Empty).Trim().ToUpperInvariant();
            if (gstin.Length != 15)
                return Refuse(period,
                    "The company GSTIN is not set on the invoice profile, so no document can be numbered. "
                  + "Fill in Invoice Settings first.");

            var request = new InvoiceGenerationRequest
            {
                PeriodFrom = from,
                PeriodTo = to,
                CompanyGSTIN = gstin,
                // The first two digits of our own GSTIN are the statutory state code, and they
                // cannot disagree with the registration the documents are raised under.
                SupplierStateCode = gstin[..2],
                OnlyPartyUserID = 0,
                CreatedByUserID = createdByUserID,
                DryRun = true,
                Consolidation = consolidation ?? string.Empty,
            };

            // Remembered only when documents are actually issued. A preview is somebody looking;
            // it should not change how next month is invoiced behind their back.
            if (saveAsDefault && !dryRun && !string.IsNullOrWhiteSpace(consolidation))
            {
                string value = InvoiceConsolidation.Normalise(consolidation);
                var row = await db.SettingMasters
                    .FirstOrDefaultAsync(s => s.Key == InvoiceConsolidation.SettingKey && !s.IsDeleted);
                if (row != null && !string.Equals(row.Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    row.Value = value;
                    row.UpdatedateTime = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                    _log.LogInformation("Invoice grouping default changed to {Value}.", value);
                }
            }

            var previewCommission = await invoices.GenerateCommissionInvoices(request);
            var previewTopUps = await invoices.GenerateWalletTopUpInvoices(request);

            var blockers = previewCommission.Warnings.Concat(previewTopUps.Warnings)
                .Where(w => w.Contains("No place of supply", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            var result = new MonthlyInvoiceResult
            {
                PeriodKey = period,
                PeriodFrom = from,
                PeriodTo = to,
                CompanyGSTIN = gstin,
                DryRun = dryRun,
                Consolidation = string.IsNullOrWhiteSpace(consolidation)
                    ? await ResolveDefaultConsolidation(db)
                    : InvoiceConsolidation.Normalise(consolidation),
                DocumentsGenerated = previewCommission.DocumentsGenerated + previewTopUps.DocumentsGenerated,
                TaxableTotal = previewCommission.TaxableTotal + previewTopUps.TaxableTotal,
                TaxTotal = previewCommission.TaxTotal + previewTopUps.TaxTotal,
                SkippedAlreadyInvoiced = previewCommission.SkippedAlreadyInvoiced + previewTopUps.SkippedAlreadyInvoiced,
                Blockers = blockers,
                Warnings = previewCommission.Warnings.Concat(previewTopUps.Warnings)
                    .Where(w => !blockers.Contains(w)).Distinct().ToList(),
                Documents = previewTopUps.Documents.Concat(previewCommission.Documents).ToList(),
                PartialMonth = partialMonth,
            };
            await NameServices(db, result.Documents, previewTopUps.Documents.Count);

            if (blockers.Count > 0)
            {
                result.Refused = true;
                result.RefusalReason =
                    $"{blockers.Count} part{(blockers.Count == 1 ? "y" : "ies")} due a document for this month "
                  + $"ha{(blockers.Count == 1 ? "s" : "ve")} no place of supply. A document keeps the state it is stamped with and the portal "
                  + "rejects a blank one, so nothing was issued. Set them on the Place of Supply screen.";
                return result;
            }

            if (dryRun || result.DocumentsGenerated == 0) return result;

            request.DryRun = false;
            var commission = await invoices.GenerateCommissionInvoices(request);
            var topUps = await invoices.GenerateWalletTopUpInvoices(request);

            result.DocumentsGenerated = commission.DocumentsGenerated + topUps.DocumentsGenerated;
            result.LinesGenerated = commission.LinesGenerated + topUps.LinesGenerated;
            result.TaxableTotal = commission.TaxableTotal + topUps.TaxableTotal;
            result.TaxTotal = commission.TaxTotal + topUps.TaxTotal;
            result.SkippedAlreadyInvoiced = commission.SkippedAlreadyInvoiced + topUps.SkippedAlreadyInvoiced;
            result.Documents = topUps.Documents.Concat(commission.Documents).ToList();
            await NameServices(db, result.Documents, topUps.Documents.Count);
            result.Issued = true;
            return result;
        }

        /// <summary>
        /// Re-issue a month under a different grouping: cancel its sale documents, then generate
        /// again. The only way to turn a month already invoiced per day into one invoice per
        /// customer, because a document's content is never edited once issued.
        ///
        /// Numbers are CANCELLED, never deleted. GSTR-1 table 13 declares the numbers a series
        /// issued, and a number that vanishes makes the sequence impossible to demonstrate; the
        /// annexure lines stay too, as the record of what the withdrawn number covered.
        ///
        /// It refuses once the month has been FILED — a declaration already made is corrected
        /// with a credit note, not by withdrawing the document behind it — and once the ledger
        /// has sealed the period.
        ///
        /// This is the sharpest tool here: it withdraws documents a customer may already hold.
        /// Hence the dry run, the Admin-only policy, and the count reported before anything moves.
        /// </summary>
        public async Task<MonthlyInvoiceResult> Regroup(
            string periodKey, string? consolidation, bool dryRun, int createdByUserID, bool saveAsDefault)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            string period = (periodKey ?? string.Empty).Trim();
            if (period.Length != 6 || !int.TryParse(period, out _))
                return Refuse(period, "That is not a yyyyMM period.");

            var profile = await db.InvoiceSettings.AsNoTracking()
                .OrderBy(x => x.InvoiceSettingID).FirstOrDefaultAsync();
            string gstin = (profile?.GSTIN ?? string.Empty).Trim().ToUpperInvariant();
            if (gstin.Length != 15)
                return Refuse(period, "The company GSTIN is not set on the invoice profile.");

            bool filed = await db.GstReturnFilings.AsNoTracking()
                .AnyAsync(f => !f.IsDeleted && f.CompanyGSTIN == gstin && f.PeriodKey == period
                            && f.ReturnType == GstReturnType.Gstr1 && f.Status == GstFilingStatus.Filed);
            if (filed)
                return Refuse(period,
                    "GSTR-1 for this period has already been filed, so its documents cannot be withdrawn. "
                  + "A filed declaration is corrected with a credit note, not by cancelling the invoice.");

            var live = await db.TaxInvoiceHeaders
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Sale
                         && h.CompanyGSTIN == gstin && h.PeriodKey == period
                         && h.Status == TaxDocumentStatus.Issued
                         && h.DocumentType != TaxDocumentType.SelfInvoice)
                .ToListAsync();

            if (live.Count == 0)
            {
                // Nothing to withdraw — so this is just a generate, which is the kindest reading
                // of "re-issue this month" when the month was never issued.
                return await Generate(period, dryRun, createdByUserID, consolidation, saveAsDefault);
            }

            if (dryRun)
            {
                var preview = await Generate(period, true, createdByUserID, consolidation, saveAsDefault);
                preview.Warnings.Insert(0,
                    $"{live.Count} document(s) already issued for this period would be CANCELLED first "
                  + $"({live.Sum(h => h.GrandTotal):N2} of invoice value), and the month re-issued under the new "
                  + "grouping. Their numbers are kept and reported as cancelled in GSTR-1 table 13.");
                return preview;
            }

            // Cancels the headers AND releases the sources they were holding, so the regeneration
            // below has something to invoice. The two halves share a transaction deliberately: a
            // header cancelled without its sources released is a month that can never be
            // re-issued.
            await DocumentCancellation.Cancel(db, live,
                "Cancelled: re-issued under a different invoice grouping.");
            await db.SaveChangesAsync();
            _log.LogWarning(
                "Regroup {Period}: cancelled {Count} issued sale document(s) ({Value:N2}) to re-issue under "
              + "grouping {Grouping}. Numbers kept for GSTR-1 table 13.",
                period, live.Count, live.Sum(h => h.GrandTotal), InvoiceConsolidation.Normalise(consolidation));

            var result = await Generate(period, false, createdByUserID, consolidation, saveAsDefault);
            result.CancelledDocuments = live.Count;
            result.Warnings.Insert(0,
                $"{live.Count} earlier document(s) for this period were cancelled and replaced. Their numbers are "
              + "kept and will be reported as cancelled in GSTR-1 table 13.");
            return result;
        }

        /// <summary>The install's standing answer, so the result can report what the run used
        /// rather than the empty string the caller sent.</summary>
        /// <summary>
        /// Put a plain name on each previewed document: "Wallet top-up" for the first
        /// <paramref name="topUpCount"/> (they come first in the list), the service family for the
        /// rest, read from the SAC the engine stamped. Display only — nothing is stored from it.
        /// </summary>
        private static async Task NameServices(
            ApplicationDbContext db, List<FintechApp.Responses.TaxInvoice.GeneratedDocument> docs, int topUpCount)
        {
            try
            {
                var sacs = docs.Select(d => d.SACCode).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                var names = await db.HSNSACMasters.AsNoTracking()
                    .Where(h => !h.IsDeleted && sacs.Contains(h.Code))
                    .Select(h => new { h.Code, h.Description }).ToListAsync();
                var bySac = names.GroupBy(n => n.Code).ToDictionary(g => g.Key, g => g.First().Description);

                for (int i = 0; i < docs.Count; i++)
                {
                    var d = docs[i];
                    if (i < topUpCount) { d.ServiceName = "Wallet top-up"; continue; }
                    d.ServiceName = bySac.TryGetValue(d.SACCode, out var n) && !string.IsNullOrWhiteSpace(n)
                        ? n
                        : (string.IsNullOrWhiteSpace(d.BillingBasis) ? "Service" : d.BillingBasis.ToLowerInvariant());
                }
            }
            catch { /* a missing label must never stop an invoice run */ }
        }

        private static async Task<string> ResolveDefaultConsolidation(ApplicationDbContext db)
        {
            try
            {
                var row = await db.SettingMasters.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Key == InvoiceConsolidation.SettingKey && !s.IsDeleted);
                return row == null ? InvoiceConsolidation.Auto : InvoiceConsolidation.Normalise(row.Value);
            }
            catch { return InvoiceConsolidation.Auto; }
        }

        private static MonthlyInvoiceResult Refuse(string period, string why) => new()
        {
            PeriodKey = period ?? string.Empty,
            Refused = true,
            RefusalReason = why,
        };
    }
}
