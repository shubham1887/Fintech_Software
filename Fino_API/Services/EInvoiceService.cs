using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.TaxInvoice;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <summary>See <see cref="IEInvoiceService"/>.</summary>
    public class EInvoiceService : IEInvoiceService
    {
        /// <summary>
        /// The IRP refuses a document older than 30 days from its own date (for taxpayers at or
        /// above Rs 10 crore AATO, which this turnover is). Past it there is no IRN to be had.
        /// </summary>
        private const int ReportingWindowDays = 30;

        /// <summary>An IRN can only be cancelled within 24 hours of issue; after that the only
        /// correction is a credit note.</summary>
        private const int CancellationWindowHours = 24;

        private readonly ApplicationDbContext _context;
        private readonly IEInvoiceGateway _gateway;

        public EInvoiceService(ApplicationDbContext context, IEInvoiceGateway gateway)
        {
            _context = context;
            _gateway = gateway;
        }

        // ── Queueing ────────────────────────────────────────────────────────────────────

        public async Task<(bool Queued, string Message)> Enqueue(int taxInvoiceHeaderID)
        {
            var header = await _context.TaxInvoiceHeaders.AsNoTracking()
                .FirstOrDefaultAsync(h => h.TaxInvoiceHeaderID == taxInvoiceHeaderID && !h.IsDeleted);
            if (header == null) return (false, $"Document {taxInvoiceHeaderID} not found.");

            if (!string.Equals(header.Direction, TaxDirection.Sale, StringComparison.OrdinalIgnoreCase))
                return (false, "Only our own documents are registered with the IRP. A supplier's invoice already "
                             + "carries their IRN.");

            if (!string.Equals(header.Status, TaxDocumentStatus.Issued, StringComparison.OrdinalIgnoreCase))
                return (false, $"Document {header.DocumentNo} is {header.Status}. Only an issued document is reported.");

            // Idempotent, so the issuing path can call this without looking first.
            bool exists = await _context.EInvoiceLogs
                .AnyAsync(l => !l.IsDeleted && l.TaxInvoiceHeaderID == taxInvoiceHeaderID);
            if (exists) return (false, $"Document {header.DocumentNo} is already in the queue.");

            // A bill of supply carries no tax, so there is nothing for the IRP to register.
            // Recorded as NotApplicable rather than skipped, so the register shows every document
            // accounted for instead of leaving gaps somebody has to explain.
            bool applicable = !string.Equals(header.DocumentType, TaxDocumentType.BillOfSupply,
                                             StringComparison.OrdinalIgnoreCase);

            _context.EInvoiceLogs.Add(new EInvoiceLog
            {
                TaxInvoiceHeaderID = taxInvoiceHeaderID,
                Status = applicable ? EInvoiceStatus.Pending : EInvoiceStatus.NotApplicable,
                ReportingDeadline = applicable ? header.DocumentDate.Date.AddDays(ReportingWindowDays) : null,
                ErrorMessage = applicable ? string.Empty : "Bill of supply - no tax, so the IRP does not take it.",
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();

            return (applicable, applicable
                ? $"Queued {header.DocumentNo}; the IRP deadline is {header.DocumentDate.Date.AddDays(ReportingWindowDays):dd-MMM-yyyy}."
                : $"{header.DocumentNo} is a bill of supply and needs no IRN.");
        }

        public async Task<int> EnqueuePeriod(DateTime periodFrom, DateTime periodTo)
        {
            DateTime periodStart = periodFrom.Date;
            DateTime periodEnd = periodTo.Date.AddDays(1);

            var ids = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted
                         && h.Direction == TaxDirection.Sale
                         && h.Status == TaxDocumentStatus.Issued
                         && h.DocumentDate >= periodStart && h.DocumentDate < periodEnd
                         && !_context.EInvoiceLogs.Any(l => !l.IsDeleted && l.TaxInvoiceHeaderID == h.TaxInvoiceHeaderID))
                .Select(h => h.TaxInvoiceHeaderID)
                .ToListAsync();

            int queued = 0;
            foreach (int id in ids)
            {
                var (ok, _) = await Enqueue(id);
                if (ok) queued++;
            }
            return queued;
        }

        // ── Processing ──────────────────────────────────────────────────────────────────

        public async Task<EInvoiceProcessResult> ProcessQueue(int maxDocuments)
        {
            var result = new EInvoiceProcessResult();
            DateTime today = DateTime.Today;

            if (!_gateway.IsConfigured)
                result.Messages.Add("No IRP is configured, so every attempt will fail. The documents stay queued "
                                  + "and their deadlines keep counting down - configure the gateway.");

            // Soonest deadline first: the ones about to expire are the ones worth the attempt.
            var pending = await _context.EInvoiceLogs
                .Where(l => !l.IsDeleted && l.Status == EInvoiceStatus.Pending)
                .OrderBy(l => l.ReportingDeadline)
                .Take(Math.Max(1, maxDocuments))
                .ToListAsync();

            foreach (var log in pending)
            {
                result.Attempted++;

                if (log.ReportingDeadline.HasValue && log.ReportingDeadline.Value.Date < today)
                {
                    // The portal will not take it. Say so permanently rather than retrying for ever
                    // against a door that is now closed.
                    log.Status = EInvoiceStatus.Failed;
                    log.ErrorCode = "DEADLINE_PASSED";
                    log.ErrorMessage = $"The 30-day reporting deadline passed on "
                                     + $"{log.ReportingDeadline.Value:dd-MMM-yyyy}. No IRN can be obtained for this "
                                     + "document. The remaining route is a credit note and a fresh invoice.";
                    log.LastAttemptOn = DateTime.UtcNow;
                    log.UpdatedateTime = DateTime.UtcNow;
                    result.PastDeadline++;
                    continue;
                }

                var header = await _context.TaxInvoiceHeaders.AsNoTracking()
                    .FirstOrDefaultAsync(h => h.TaxInvoiceHeaderID == log.TaxInvoiceHeaderID);
                var lines = await _context.TaxInvoiceLines.AsNoTracking()
                    .Where(l => l.TaxInvoiceHeaderID == log.TaxInvoiceHeaderID && !l.IsDeleted)
                    .OrderBy(l => l.LineNo).ToListAsync();

                if (header == null)
                {
                    log.Status = EInvoiceStatus.Failed;
                    log.ErrorCode = "NO_DOCUMENT";
                    log.ErrorMessage = "The document this log points at no longer exists.";
                    log.UpdatedateTime = DateTime.UtcNow;
                    result.FailedPermanent++;
                    continue;
                }

                string payload = BuildPayload(header, lines);
                log.RequestPayload = payload;
                log.AttemptCount++;
                log.LastAttemptOn = DateTime.UtcNow;

                var reply = await _gateway.Register(payload);
                log.ResponsePayload = reply.RawResponse;

                if (reply.Success)
                {
                    log.Status = EInvoiceStatus.Registered;
                    log.IRN = reply.IRN;
                    log.AckNo = reply.AckNo;
                    log.AckDate = reply.AckDate;
                    log.SignedQRCode = reply.SignedQRCode;
                    log.ErrorCode = string.Empty;
                    log.ErrorMessage = string.Empty;
                    log.CancellationWindowEnds = (reply.AckDate ?? DateTime.UtcNow).AddHours(CancellationWindowHours);
                    result.Registered++;
                }
                else
                {
                    log.ErrorCode = reply.ErrorCode;
                    log.ErrorMessage = reply.ErrorMessage;
                    if (reply.IsRetryable)
                    {
                        // Stays Pending so the next pass picks it up and the deadline alert keeps
                        // showing it.
                        result.FailedRetryable++;
                    }
                    else
                    {
                        log.Status = EInvoiceStatus.Failed;
                        result.FailedPermanent++;
                        result.Messages.Add($"{header.DocumentNo}: {reply.ErrorCode} - {reply.ErrorMessage}");
                    }
                }

                log.UpdatedateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// The IRP's JSON for one document.
        ///
        /// Built from the document's OWN stored figures, never recomputed. What is reported has to
        /// be exactly what was issued and what the books carry; a payload that recalculates would
        /// drift from the invoice the moment a rate changed.
        /// </summary>
        private static string BuildPayload(TaxInvoiceHeader h, List<TaxInvoiceLine> lines) =>
            JsonConvert.SerializeObject(new
            {
                Version = "1.1",
                TranDtls = new
                {
                    TaxSch = "GST",
                    SupTyp = h.PartyRegistrationStatus == PartyRegistrationStatus.Registered ? "B2B" : "B2C",
                    RegRev = h.DocumentType == TaxDocumentType.SelfInvoice ? "Y" : "N",
                },
                DocDtls = new
                {
                    Typ = h.DocumentType == TaxDocumentType.CreditNote ? "CRN"
                        : h.DocumentType == TaxDocumentType.DebitNote ? "DBN" : "INV",
                    No = h.DocumentNo,
                    Dt = h.DocumentDate.ToString("dd/MM/yyyy"),
                },
                SellerDtls = new { Gstin = h.CompanyGSTIN, Stcd = h.SupplierStateCode },
                BuyerDtls = new { Gstin = h.PartyGSTIN, TrdNm = h.PartyName, Pos = h.PlaceOfSupplyStateCode },
                ItemList = lines.Select((l, i) => new
                {
                    SlNo = (i + 1).ToString(),
                    PrdDesc = l.Description,
                    HsnCd = l.SACCode,
                    Qty = l.Quantity,
                    AssAmt = l.TaxableValue,
                    GstRt = l.CGSTRate + l.SGSTRate + l.IGSTRate,
                    CgstAmt = l.CGSTAmount,
                    SgstAmt = l.SGSTAmount,
                    IgstAmt = l.IGSTAmount,
                    CesAmt = l.CessAmount,
                    TotItemVal = l.LineTotal,
                }),
                ValDtls = new
                {
                    AssVal = h.TaxableTotal,
                    CgstVal = h.CGSTTotal,
                    SgstVal = h.SGSTTotal,
                    IgstVal = h.IGSTTotal,
                    CesVal = h.CessTotal,
                    RndOffAmt = h.RoundOff,
                    TotInvVal = h.GrandTotal,
                },
            });

        // ── Alerts ──────────────────────────────────────────────────────────────────────

        public async Task<List<EInvoiceDeadlineAlert>> GetDeadlineAlerts(DateTime asOf, int daysAhead)
        {
            DateTime cutoff = asOf.Date.AddDays(Math.Max(0, daysAhead));

            var rows = await (from l in _context.EInvoiceLogs.AsNoTracking()
                              join h in _context.TaxInvoiceHeaders.AsNoTracking()
                                   on l.TaxInvoiceHeaderID equals h.TaxInvoiceHeaderID
                              where !l.IsDeleted
                                 && l.Status == EInvoiceStatus.Pending
                                 && l.ReportingDeadline != null
                                 && l.ReportingDeadline <= cutoff
                              select new
                              {
                                  l.EInvoiceLogID, l.TaxInvoiceHeaderID, l.ReportingDeadline,
                                  l.AttemptCount, l.ErrorMessage,
                                  h.DocumentNo, h.DocumentDate, h.PartyName, h.GrandTotal,
                              }).ToListAsync();

            return rows.Select(r => new EInvoiceDeadlineAlert
            {
                EInvoiceLogID = r.EInvoiceLogID,
                TaxInvoiceHeaderID = r.TaxInvoiceHeaderID,
                DocumentNo = r.DocumentNo,
                DocumentDate = r.DocumentDate,
                PartyName = r.PartyName,
                GrandTotal = r.GrandTotal,
                ReportingDeadline = r.ReportingDeadline!.Value,
                DaysLeft = (int)(r.ReportingDeadline!.Value.Date - asOf.Date).TotalDays,
                DeadlinePassed = r.ReportingDeadline!.Value.Date < asOf.Date,
                AttemptCount = r.AttemptCount,
                LastError = r.ErrorMessage,
            })
            .OrderBy(a => a.ReportingDeadline)
            .ToList();
        }
    }
}
