using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TaxInvoice;
using FintechApp.Responses.TaxInvoice;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <summary>See <see cref="IExchangeLegService"/>.</summary>
    public class ExchangeLegService : IExchangeLegService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxInvoiceService _invoices;

        public ExchangeLegService(ApplicationDbContext context, ITaxInvoiceService invoices)
        {
            _context = context;
            _invoices = invoices;
        }

        // ── Detection ───────────────────────────────────────────────────────────────────

        public async Task<ExchangeLegDetectionResult> DetectLegs(InvoiceGenerationRequest request)
        {
            var result = new ExchangeLegDetectionResult
            {
                DryRun = request.DryRun,
                PeriodFrom = request.PeriodFrom.Date,
                PeriodTo = request.PeriodTo.Date,
            };
            DateTime periodStart = request.PeriodFrom.Date;
            DateTime periodEnd = request.PeriodTo.Date.AddDays(1);

            var partners = await _context.PartyTaxProfiles.AsNoTracking()
                .Where(p => !p.IsDeleted && p.IsExchangePartner
                         && (request.OnlyPartyUserID == 0 || p.UserID == request.OnlyPartyUserID))
                .Select(p => p.UserID).ToListAsync();

            if (partners.Count == 0)
            {
                result.Warnings.Add("No party is flagged as an exchange partner, so nothing was detected. "
                                  + "Set PartyTaxProfiles.IsExchangePartner on the platforms you swap wallet credit with — "
                                  + "until then their barter turnover is invisible, which is exactly how it goes missing.");
                return result;
            }

            int adminId = await AdminUserId();
            if (adminId == 0)
            {
                result.Warnings.Add("No admin user found, so our own side of a swap cannot be identified.");
                return result;
            }

            // Both halves appear as wallet movements in this system: the partner holds an account
            // here, so admin -> partner is our supply and partner -> admin is theirs.
            var movements = await _context.FundTransactions.AsNoTracking()
                .Where(f => !f.IsDeleted
                         && f.DateTime >= periodStart && f.DateTime < periodEnd
                         && ((f.SenderID == adminId && partners.Contains(f.ReceiverID))
                          || (f.ReceiverID == adminId && partners.Contains(f.SenderID))))
                .Select(f => new { f.FundTransactionID, f.SenderID, f.ReceiverID, f.WalletID, f.Amount, f.DateTime, f.Status })
                .ToListAsync();

            movements = movements
                .Where(m => string.Equals(m.Status, StatusType.Success, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (movements.Count == 0) return result;

            var ids = movements.Select(m => m.FundTransactionID).ToList();
            var known = await _context.ExchangePartnerLegs.AsNoTracking()
                .Where(l => !l.IsDeleted && ids.Contains(l.FundTransactionID))
                .Select(l => new { l.FundTransactionID, l.Direction })
                .ToListAsync();
            var knownKeys = known.Select(k => $"{k.FundTransactionID}|{k.Direction.ToUpperInvariant()}").ToHashSet();

            foreach (var m in movements)
            {
                bool outward = m.SenderID == adminId;
                string direction = outward ? ExchangeLegDirection.Outward : ExchangeLegDirection.Inward;
                int partnerId = outward ? m.ReceiverID : m.SenderID;

                if (knownKeys.Contains($"{m.FundTransactionID}|{direction}"))
                {
                    result.LegsAlreadyKnown++;
                    continue;
                }

                if (!request.DryRun)
                {
                    _context.ExchangePartnerLegs.Add(new ExchangePartnerLeg
                    {
                        PartnerUserID = partnerId,
                        Direction = direction,
                        FundTransactionID = m.FundTransactionID,
                        WalletID = m.WalletID,
                        LegDate = m.DateTime,
                        Amount = m.Amount,
                        TaxInvoiceHeaderID = null,
                        DocumentStatus = ExchangeDocumentStatus.AwaitingDocument,
                        PeriodKey = m.DateTime.ToString("yyyyMM"),
                        Remark = outward
                            ? "Our supply - invoice to be raised"
                            : "Partner's supply - their invoice to be obtained and entered",
                        DateTime = DateTime.UtcNow,
                        UpdatedateTime = DateTime.UtcNow,
                    });
                }

                if (outward) result.OutwardLegsCreated++; else result.InwardLegsCreated++;
            }

            if (!request.DryRun) await _context.SaveChangesAsync();

            if (result.InwardLegsCreated > 0)
                result.Warnings.Add($"{result.InwardLegsCreated} inward leg(s) recorded awaiting the partner's invoice. "
                                  + "These cannot be generated — the document has to be obtained from the partner. "
                                  + "Until it is, the input credit on that supply cannot be claimed.");

            return result;
        }

        // ── Enforcement ─────────────────────────────────────────────────────────────────

        public async Task<InvoiceGenerationResult> InvoiceOutwardLegs(InvoiceGenerationRequest request)
        {
            var result = new InvoiceGenerationResult
            {
                DryRun = request.DryRun,
                PeriodFrom = request.PeriodFrom.Date,
                PeriodTo = request.PeriodTo.Date,
            };
            DateTime periodStart = request.PeriodFrom.Date;
            DateTime periodEnd = request.PeriodTo.Date.AddDays(1);

            var pending = await _context.ExchangePartnerLegs.AsNoTracking()
                .Where(l => !l.IsDeleted
                         && l.Direction == ExchangeLegDirection.Outward
                         && l.TaxInvoiceHeaderID == null
                         && l.LegDate >= periodStart && l.LegDate < periodEnd
                         && (request.OnlyPartyUserID == 0 || l.PartnerUserID == request.OnlyPartyUserID))
                .Select(l => l.ExchangePartnerLegID)
                .ToListAsync();

            foreach (int legId in pending)
            {
                var (doc, failure) = await _invoices.IssueExchangeLegInvoice(legId, request);
                if (doc == null)
                {
                    result.SkippedUnresolved++;
                    result.Warnings.Add($"Leg {legId}: {failure}");
                    continue;
                }

                result.DocumentsGenerated++;
                if (!request.DryRun) { result.LinesGenerated++; result.AnnexureRowsGenerated++; }
                result.GrossTotal += doc.GrossTotal;
                result.TaxableTotal += doc.TaxableTotal;
                result.TaxTotal += doc.TaxTotal;
                result.Documents.Add(doc);
            }

            return result;
        }

        // ── Register ────────────────────────────────────────────────────────────────────

        public async Task<ExchangeRegisterResult> GetRegister(DateTime periodFrom, DateTime periodTo)
        {
            DateTime periodStart = periodFrom.Date;
            DateTime periodEnd = periodTo.Date.AddDays(1);

            var legs = await (from l in _context.ExchangePartnerLegs.AsNoTracking()
                             join u in _context.UserMasters.AsNoTracking() on l.PartnerUserID equals u.UserMasterID
                             where !l.IsDeleted && l.LegDate >= periodStart && l.LegDate < periodEnd
                             select new
                             {
                                 l.PartnerUserID, l.Direction, l.Amount, l.PeriodKey,
                                 l.TaxInvoiceHeaderID, l.DocumentStatus,
                                 u.FirmName, u.PersonName, u.GSTNo,
                             }).ToListAsync();

            var result = new ExchangeRegisterResult { PeriodFrom = periodStart, PeriodTo = periodTo.Date };

            foreach (var g in legs.GroupBy(l => new { l.PartnerUserID, l.PeriodKey }))
            {
                var outward = g.Where(x => string.Equals(x.Direction, ExchangeLegDirection.Outward, StringComparison.OrdinalIgnoreCase)).ToList();
                var inward = g.Where(x => string.Equals(x.Direction, ExchangeLegDirection.Inward, StringComparison.OrdinalIgnoreCase)).ToList();
                var first = g.First();

                var row = new ExchangeRegisterRow
                {
                    PartnerUserID = g.Key.PartnerUserID,
                    PartnerName = !string.IsNullOrWhiteSpace(first.FirmName) ? first.FirmName : (first.PersonName ?? string.Empty),
                    PartnerGSTIN = first.GSTNo ?? string.Empty,
                    PeriodKey = g.Key.PeriodKey,
                    OutwardAmount = outward.Sum(x => x.Amount),
                    OutwardLegs = outward.Count,
                    InwardAmount = inward.Sum(x => x.Amount),
                    InwardLegs = inward.Count,
                    OutwardAwaitingDocument = outward.Count(x => x.TaxInvoiceHeaderID == null
                        && !string.Equals(x.DocumentStatus, ExchangeDocumentStatus.WrittenOff, StringComparison.OrdinalIgnoreCase)),
                    InwardAwaitingDocument = inward.Count(x => x.TaxInvoiceHeaderID == null
                        && !string.Equals(x.DocumentStatus, ExchangeDocumentStatus.WrittenOff, StringComparison.OrdinalIgnoreCase)),
                    InwardAwaitingAmount = inward.Where(x => x.TaxInvoiceHeaderID == null
                        && !string.Equals(x.DocumentStatus, ExchangeDocumentStatus.WrittenOff, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.Amount),
                };

                // Net is what makes a swap look like nothing happened; turnover is what the return
                // needs. Both are reported, side by side, so the difference cannot be overlooked.
                row.NetPosition = row.OutwardAmount - row.InwardAmount;
                row.TotalTurnover = row.OutwardAmount + row.InwardAmount;

                result.Rows.Add(row);
                result.TotalOutward += row.OutwardAmount;
                result.TotalInward += row.InwardAmount;
                result.TotalAwaitingDocument += row.OutwardAwaitingDocument + row.InwardAwaitingDocument;
                result.TotalAwaitingAmount += row.InwardAwaitingAmount;
            }

            result.TotalTurnover = result.TotalOutward + result.TotalInward;
            result.Rows = result.Rows.OrderByDescending(r => r.TotalTurnover).ToList();
            return result;
        }

        private async Task<int> AdminUserId() =>
            await _context.UserMasters.AsNoTracking()
                .Where(u => u.UserTypeID == UserTypeCode.Admin)
                .OrderBy(u => u.UserMasterID)
                .Select(u => u.UserMasterID)
                .FirstOrDefaultAsync();
    }
}
