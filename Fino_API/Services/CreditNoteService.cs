using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses.TaxInvoice;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <summary>See <see cref="ICreditNoteService"/>.</summary>
    public class CreditNoteService : ICreditNoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxDocumentNumberService _numbers;

        public CreditNoteService(ApplicationDbContext context, ITaxDocumentNumberService numbers)
        {
            _context = context;
            _numbers = numbers;
        }

        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// The last date a credit note can still carry tax: 30 November of the year AFTER the
        /// supply's financial year. (The annual return, if filed earlier, closes it sooner — that
        /// date is not known here, so this is the outer limit and the earlier one has to be
        /// watched by whoever files.)
        ///
        /// A supply on 30-Sep-2026 is in FY 2026-27, so the window closes 30-Nov-2027.
        /// </summary>
        public static DateTime WindowCloseFor(DateTime supplyDate)
        {
            int fyStart = supplyDate.Month >= 4 ? supplyDate.Year : supplyDate.Year - 1;
            return new DateTime(fyStart + 1, 11, 30);
        }

        public async Task<(GeneratedDocument? Document, string FailureMessage)> Issue(CreditNoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return (null, "A reason is required. A credit note without one cannot be defended in an audit.");

            var original = await _context.TaxInvoiceHeaders
                .FirstOrDefaultAsync(h => h.TaxInvoiceHeaderID == request.OriginalInvoiceHeaderID && !h.IsDeleted);
            if (original == null) return (null, $"Document {request.OriginalInvoiceHeaderID} not found.");

            if (!string.Equals(original.Direction, TaxDirection.Sale, StringComparison.OrdinalIgnoreCase))
                return (null, "Only our own sale documents can be credited here. A supplier's credit note is "
                            + "entered on the purchase side.");

            if (!string.Equals(original.Status, TaxDocumentStatus.Issued, StringComparison.OrdinalIgnoreCase))
                return (null, $"Document {original.DocumentNo} is {original.Status}, not Issued. "
                            + "There is nothing to reverse.");

            if (string.Equals(original.DocumentType, TaxDocumentType.CreditNote, StringComparison.OrdinalIgnoreCase)
             || string.Equals(original.DocumentType, TaxDocumentType.DebitNote, StringComparison.OrdinalIgnoreCase))
                return (null, $"Document {original.DocumentNo} is itself a {original.DocumentType}. "
                            + "Credit the invoice it was raised against, not the note.");

            // How much of this document is still reversible. Cumulative, because a consolidated
            // invoice is commonly reversed one source at a time and nothing may be credited twice.
            decimal alreadyCredited = await _context.TaxInvoiceHeaders
                .Where(h => !h.IsDeleted
                         && h.OriginalInvoiceHeaderID == original.TaxInvoiceHeaderID
                         && h.DocumentType == TaxDocumentType.CreditNote
                         && h.Status == TaxDocumentStatus.Issued)
                .SumAsync(h => (decimal?)h.GrossTotal) ?? 0m;

            decimal remaining = R2(original.GrossTotal - alreadyCredited);
            if (remaining <= 0m)
                return (null, $"Document {original.DocumentNo} is already fully credited "
                            + $"({alreadyCredited:N2} of {original.GrossTotal:N2}).");

            decimal gross = request.GrossAmount <= 0m ? remaining : R2(request.GrossAmount);
            if (gross > remaining)
                return (null, $"Asked to credit {gross:N2} but only {remaining:N2} of "
                            + $"{original.DocumentNo} remains (already credited {alreadyCredited:N2}).");

            DateTime noteDate = (request.NoteDate ?? DateTime.Today).Date;
            if (noteDate < original.DocumentDate.Date)
                return (null, $"A credit note cannot predate the document it reverses "
                            + $"({original.DocumentDate:dd-MMM-yyyy}).");

            DateTime windowCloses = WindowCloseFor(original.DocumentDate);
            bool windowClosed = noteDate > windowCloses;

            // Pro-rate from the ORIGINAL's own figures. A reversal carries the rate that was
            // actually charged; re-resolving against today's masters would credit an amount that
            // was never collected if a rate changed in between.
            decimal share = original.GrossTotal == 0m ? 0m : gross / original.GrossTotal;
            decimal taxable = R2(original.TaxableTotal * share);
            decimal cgst = windowClosed ? 0m : R2(original.CGSTTotal * share);
            decimal sgst = windowClosed ? 0m : R2(original.SGSTTotal * share);
            decimal igst = windowClosed ? 0m : R2(original.IGSTTotal * share);
            decimal cess = windowClosed ? 0m : R2(original.CessTotal * share);
            decimal tax = R2(cgst + sgst + igst + cess);
            decimal roundOff = R2(gross - taxable - tax);

            var doc = new GeneratedDocument
            {
                DocumentType = TaxDocumentType.CreditNote,
                DocumentDate = noteDate,
                PartyUserID = original.PartyUserID,
                PartyName = original.PartyName,
                PartyRegistrationStatus = original.PartyRegistrationStatus,
                SourceCount = 1,
                GrossTotal = gross,
                TaxableTotal = taxable,
                TaxTotal = tax,
                GrandTotal = R2(taxable + tax + roundOff),
                BillingBasis = original.SupplyNature,
            };

            if (request.DryRun)
            {
                doc.DocumentNo = "(dry run)";
                return (doc, windowClosed ? WindowClosedWarning(original, windowCloses) : string.Empty);
            }

            string gstin = !string.IsNullOrWhiteSpace(request.CompanyGSTIN) ? request.CompanyGSTIN : original.CompanyGSTIN;
            var (documentNo, failure) = await _numbers.Allocate(
                TaxDocumentSeriesCode.For(TaxDocumentType.CreditNote), gstin, noteDate);
            if (failure.Length > 0) return (null, failure);

            var note = new TaxInvoiceHeader
            {
                Direction = TaxDirection.Sale,
                DocumentType = TaxDocumentType.CreditNote,
                DocumentNo = documentNo,
                DocumentDate = noteDate,
                PartyUserID = original.PartyUserID,
                PartyName = original.PartyName,
                PartyGSTIN = original.PartyGSTIN,
                PartyRegistrationStatus = original.PartyRegistrationStatus,
                CompanyGSTIN = gstin,
                SupplierStateCode = original.SupplierStateCode,
                PlaceOfSupplyStateCode = original.PlaceOfSupplyStateCode,
                SupplyNature = original.SupplyNature,
                GrossTotal = gross,
                TaxableTotal = taxable,
                CGSTTotal = cgst,
                SGSTTotal = sgst,
                IGSTTotal = igst,
                CessTotal = cess,
                RoundOff = roundOff,
                GrandTotal = R2(taxable + tax + roundOff),
                OriginalInvoiceHeaderID = original.TaxInvoiceHeaderID,
                NoteReason = request.Reason.Trim(),
                NoteTimeLimitDate = windowCloses,
                Status = TaxDocumentStatus.Issued,
                PeriodKey = noteDate.ToString("yyyyMM"),
                IsConsolidated = false,
                Remark = windowClosed
                    ? $"Window closed {windowCloses:dd-MMM-yyyy} - issued WITHOUT tax"
                    : $"Against {original.DocumentNo} dated {original.DocumentDate:dd-MMM-yyyy}",
                CreatedByUserID = request.CreatedByUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };
            _context.TaxInvoiceHeaders.Add(note);
            await _context.SaveChangesAsync();

            // One line carrying the original's SAC and rates, so the note prints and reports the
            // same way the invoice did.
            var originalLine = await _context.TaxInvoiceLines.AsNoTracking()
                .Where(l => l.TaxInvoiceHeaderID == original.TaxInvoiceHeaderID && !l.IsDeleted)
                .OrderBy(l => l.LineNo).FirstOrDefaultAsync();

            _context.TaxInvoiceLines.Add(new TaxInvoiceLine
            {
                TaxInvoiceHeaderID = note.TaxInvoiceHeaderID,
                LineNo = 1,
                ServiceID = originalLine?.ServiceID ?? 0,
                Description = $"Credit against {original.DocumentNo}: {request.Reason.Trim()}",
                SACCode = originalLine?.SACCode ?? string.Empty,
                Quantity = 1,
                BillingBasis = originalLine?.BillingBasis ?? string.Empty,
                GrossValue = gross,
                TaxableValue = taxable,
                CGSTRate = windowClosed ? 0m : originalLine?.CGSTRate ?? 0m,
                CGSTAmount = cgst,
                SGSTRate = windowClosed ? 0m : originalLine?.SGSTRate ?? 0m,
                SGSTAmount = sgst,
                IGSTRate = windowClosed ? 0m : originalLine?.IGSTRate ?? 0m,
                IGSTAmount = igst,
                CessAmount = cess,
                LineTotal = R2(taxable + tax + roundOff),
                TaxRuleMasterID = originalLine?.TaxRuleMasterID ?? 0,
                ITCEligible = false,
                ReverseCharge = originalLine?.ReverseCharge ?? false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();

            doc.TaxInvoiceHeaderID = note.TaxInvoiceHeaderID;
            doc.DocumentNo = documentNo;
            return (doc, windowClosed ? WindowClosedWarning(original, windowCloses) : string.Empty);
        }

        private static string WindowClosedWarning(TaxInvoiceHeader original, DateTime windowCloses) =>
            $"The credit-note window for {original.DocumentNo} closed on {windowCloses:dd-MMM-yyyy}. "
          + "The note was issued WITHOUT tax - commercially it still reduces what the party owes, "
          + "but the GST on that supply can no longer be recovered.";

        public async Task<List<CreditNoteWindowAlert>> GetWindowAlerts(DateTime asOf, int daysAhead)
        {
            DateTime cutoff = asOf.Date.AddDays(Math.Max(0, daysAhead));

            // Sale documents that cover at least one source which did not succeed. The join is the
            // whole point: a window closing on a document where everything worked needs no action,
            // and an alert that fires on those is an alert people learn to ignore.
            var candidates = await (
                from h in _context.TaxInvoiceHeaders.AsNoTracking()
                where !h.IsDeleted
                   && h.Direction == TaxDirection.Sale
                   && h.Status == TaxDocumentStatus.Issued
                   && (h.DocumentType == TaxDocumentType.TaxInvoice || h.DocumentType == TaxDocumentType.BillOfSupply)
                select new { h.TaxInvoiceHeaderID, h.DocumentNo, h.DocumentDate, h.PartyUserID, h.PartyName, h.GrossTotal })
                .ToListAsync();
            if (candidates.Count == 0) return new List<CreditNoteWindowAlert>();

            var ids = candidates.Select(c => c.TaxInvoiceHeaderID).ToList();

            var failedBySource = await (
                from a in _context.TaxInvoiceAnnexureLines.AsNoTracking()
                join t in _context.TransactionMasters.AsNoTracking() on a.TransactionMasterID equals t.TransactionMasterID
                where !a.IsDeleted && a.TransactionMasterID > 0 && ids.Contains(a.TaxInvoiceHeaderID)
                   && (t.Status == "FAILED" || t.Status == "REFUND" || t.Status == "Reversed")
                select new { a.TaxInvoiceHeaderID, a.GrossValue })
                .ToListAsync();

            var creditedByOriginal = (await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.DocumentType == TaxDocumentType.CreditNote
                         && h.Status == TaxDocumentStatus.Issued
                         && h.OriginalInvoiceHeaderID != null && ids.Contains(h.OriginalInvoiceHeaderID.Value))
                .Select(h => new { OriginalID = h.OriginalInvoiceHeaderID!.Value, h.GrossTotal })
                .ToListAsync())
                .GroupBy(x => x.OriginalID)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.GrossTotal));

            var alerts = new List<CreditNoteWindowAlert>();
            foreach (var c in candidates)
            {
                var failed = failedBySource.Where(f => f.TaxInvoiceHeaderID == c.TaxInvoiceHeaderID).ToList();
                if (failed.Count == 0) continue;

                decimal credited = creditedByOriginal.TryGetValue(c.TaxInvoiceHeaderID, out var cr) ? cr : 0m;
                decimal remaining = R2(c.GrossTotal - credited);
                if (remaining <= 0m) continue;   // already dealt with

                DateTime closes = WindowCloseFor(c.DocumentDate);
                if (closes > cutoff) continue;   // not urgent yet

                alerts.Add(new CreditNoteWindowAlert
                {
                    TaxInvoiceHeaderID = c.TaxInvoiceHeaderID,
                    DocumentNo = c.DocumentNo,
                    DocumentDate = c.DocumentDate,
                    PartyUserID = c.PartyUserID,
                    PartyName = c.PartyName,
                    GrossTotal = c.GrossTotal,
                    AlreadyCredited = credited,
                    RemainingCreditable = remaining,
                    FailedSourceGross = R2(failed.Sum(f => f.GrossValue)),
                    FailedSourceCount = failed.Count,
                    WindowClosesOn = closes,
                    DaysLeft = (int)(closes - asOf.Date).TotalDays,
                    WindowClosed = closes < asOf.Date,
                });
            }

            return alerts.OrderBy(a => a.WindowClosesOn).ToList();
        }
    }
}
