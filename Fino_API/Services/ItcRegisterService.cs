using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase3;
using FintechApp.Responses.Phase3;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IItcRegisterService" />
    public class ItcRegisterService : IItcRegisterService
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = new();

        public ItcRegisterService(ApplicationDbContext context) => _context = context;

        public async Task<ItcBuildResult> BuildRegister(ItcBuildRequest request)
        {
            string periodKey = request.PeriodKey.Trim();
            var result = new ItcBuildResult { PeriodKey = periodKey };
            if (periodKey.Length != 6) return result;

            // Every purchase in the period, with its lines, so the eligible credit is the tax on the
            // lines actually marked ITC-eligible — not the whole document's tax.
            var purchases = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => !h.IsDeleted && h.Direction == TaxDirection.Purchase && h.PeriodKey == periodKey)
                .ToListAsync();
            var headerIds = purchases.Select(h => h.TaxInvoiceHeaderID).ToList();

            var linesByHeader = (await _context.TaxInvoiceLines.AsNoTracking()
                    .Where(l => headerIds.Contains(l.TaxInvoiceHeaderID) && !l.IsDeleted).ToListAsync())
                .GroupBy(l => l.TaxInvoiceHeaderID)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var h in purchases)
            {
                linesByHeader.TryGetValue(h.TaxInvoiceHeaderID, out var lines);
                lines ??= new List<TaxInvoiceLine>();

                decimal eligible = lines.Where(l => l.ITCEligible)
                    .Sum(l => l.CGSTAmount + l.SGSTAmount + l.IGSTAmount + l.CessAmount);
                decimal igst = lines.Sum(l => l.IGSTAmount);
                decimal cgst = lines.Sum(l => l.CGSTAmount);
                decimal sgst = lines.Sum(l => l.SGSTAmount);
                decimal cess = lines.Sum(l => l.CessAmount);

                var entry = await _context.ItcRegisterEntries
                    .FirstOrDefaultAsync(e => e.TaxInvoiceHeaderID == h.TaxInvoiceHeaderID && !e.IsDeleted);

                bool isNew = entry == null;
                entry ??= new ItcRegisterEntry { TaxInvoiceHeaderID = h.TaxInvoiceHeaderID, DateTime = DateTime.UtcNow };

                // A built/rebuilt entry refreshes the figures and deadlines, but NEVER overwrites a
                // claim or a reversal already recorded — those are facts about what was filed.
                if (entry.Status != ItcStatus.Claimed && entry.Status != ItcStatus.Reversed)
                    entry.Status = eligible > 0 ? ItcStatus.Eligible : ItcStatus.Ineligible;

                entry.PeriodKey = periodKey;
                entry.SupplierGSTIN = h.PartyGSTIN;
                entry.SupplierName = h.PartyName;
                entry.TaxableValue = h.TaxableTotal;
                entry.IGST = igst; entry.CGST = cgst; entry.SGST = sgst; entry.Cess = cess;
                entry.EligibleITC = eligible;
                entry.Rule37DueDate = Phase3Dates.Rule37DueDate(h.DocumentDate);
                entry.Section16_4Deadline = Phase3Dates.Section16_4Deadline(h.DocumentDate);
                entry.UpdatedateTime = DateTime.UtcNow;

                if (isNew) { _context.ItcRegisterEntries.Add(entry); result.EntriesCreated++; }
                else result.EntriesUpdated++;

                result.TotalEligibleITC += eligible;
            }
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<List<ItcRegisterRow>> GetRegister(string periodKey)
        {
            var q = _context.ItcRegisterEntries.AsNoTracking().Where(e => !e.IsDeleted);
            if (!string.IsNullOrWhiteSpace(periodKey)) q = q.Where(e => e.PeriodKey == periodKey.Trim());

            var entries = await q.OrderBy(e => e.Section16_4Deadline).ToListAsync();
            var headerIds = entries.Select(e => e.TaxInvoiceHeaderID).ToList();
            var headers = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => headerIds.Contains(h.TaxInvoiceHeaderID))
                .Select(h => new { h.TaxInvoiceHeaderID, h.DocumentNo, h.DocumentDate }).ToListAsync();
            var byId = headers.ToDictionary(x => x.TaxInvoiceHeaderID);

            return entries.Select(e => new ItcRegisterRow
            {
                ItcRegisterEntryID = e.ItcRegisterEntryID,
                TaxInvoiceHeaderID = e.TaxInvoiceHeaderID,
                DocumentNo = byId.TryGetValue(e.TaxInvoiceHeaderID, out var h) ? h.DocumentNo : string.Empty,
                DocumentDate = byId.TryGetValue(e.TaxInvoiceHeaderID, out var h2) ? h2.DocumentDate : default,
                SupplierGSTIN = e.SupplierGSTIN,
                SupplierName = e.SupplierName,
                EligibleITC = e.EligibleITC,
                Status = e.Status,
                ReconBucket = e.ReconBucket,
                ClaimedInPeriod = e.ClaimedInPeriod,
                SupplierPaid = e.SupplierPaid,
                Rule37DueDate = e.Rule37DueDate,
                Section16_4Deadline = e.Section16_4Deadline,
            }).ToList();
        }

        public async Task<string> MarkSupplierPaid(ItcMarkPaidRequest request)
        {
            var entry = await _context.ItcRegisterEntries.FirstOrDefaultAsync(e => e.ItcRegisterEntryID == request.EntryID && !e.IsDeleted);
            if (entry == null) return "That ITC entry was not found.";
            entry.SupplierPaid = true;
            entry.SupplierPaidOn = (request.PaidOn ?? DateTime.Today).Date;
            entry.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<(int Claimed, string Failure)> Claim(ItcClaimRequest request)
        {
            if (request.EntryIDs.Count == 0) return (0, "Nothing was selected to claim.");
            var entries = await _context.ItcRegisterEntries
                .Where(e => request.EntryIDs.Contains(e.ItcRegisterEntryID) && !e.IsDeleted).ToListAsync();

            int claimed = 0;
            var refused = new List<string>();
            foreach (var e in entries)
            {
                if (e.Status == ItcStatus.Claimed) continue; // already claimed, idempotent
                if (e.Status == ItcStatus.Ineligible || e.EligibleITC <= 0) { refused.Add($"#{e.ItcRegisterEntryID} has no eligible credit"); continue; }
                // A credit is only safe to claim once the document is matched in 2B. An unmatched or
                // mismatched document is credit the supplier has not (correctly) filed.
                if (e.ReconBucket != ReconBucket.Matched) { refused.Add($"#{e.ItcRegisterEntryID} is not matched in 2B ({(string.IsNullOrEmpty(e.ReconBucket) ? "not reconciled" : e.ReconBucket)})"); continue; }

                e.Status = ItcStatus.Claimed;
                e.ClaimedInPeriod = string.IsNullOrWhiteSpace(request.ClaimPeriod) ? e.PeriodKey : request.ClaimPeriod.Trim();
                e.ClaimedOn = DateTime.UtcNow;
                e.ClaimedByUserID = request.CreatedByUserID;
                e.UpdatedateTime = DateTime.UtcNow;
                claimed++;
            }
            await _context.SaveChangesAsync();

            string failure = refused.Count > 0 ? $"{claimed} claimed; {refused.Count} refused: {string.Join("; ", refused)}" : string.Empty;
            return (claimed, failure);
        }

        public async Task<string> Reverse(ItcReverseRequest request)
        {
            var entry = await _context.ItcRegisterEntries.FirstOrDefaultAsync(e => e.ItcRegisterEntryID == request.EntryID && !e.IsDeleted);
            if (entry == null) return "That ITC entry was not found.";

            string reason = request.Reason?.Trim() ?? string.Empty;
            if (reason != ItcReversalReason.Rule37 && reason != ItcReversalReason.Section16_4 && reason != ItcReversalReason.Manual)
                return "A reversal needs a reason: Rule37, Section16_4 or Manual.";

            entry.Status = ItcStatus.Reversed;
            entry.ReversalReason = reason;
            entry.ReversedOn = DateTime.UtcNow;
            entry.ReversedByUserID = request.CreatedByUserID;
            entry.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<List<ItcDeadlineAlert>> GetRule37Alerts(DateTime asOf, int daysAhead)
        {
            DateTime horizon = asOf.Date.AddDays(daysAhead);
            // Rule 37 bites a CLAIMED credit whose supplier is still unpaid as the 180 days run out.
            var entries = await _context.ItcRegisterEntries.AsNoTracking()
                .Where(e => !e.IsDeleted && e.Status == ItcStatus.Claimed && !e.SupplierPaid
                         && e.Rule37DueDate <= horizon)
                .OrderBy(e => e.Rule37DueDate).ToListAsync();

            return await Decorate(entries, e => e.Rule37DueDate, asOf, ItcReversalReason.Rule37,
                "Pay the supplier, or reverse the credit.");
        }

        public async Task<List<ItcDeadlineAlert>> GetSection16_4Alerts(DateTime asOf, int daysAhead)
        {
            DateTime horizon = asOf.Date.AddDays(daysAhead);
            // 16(4) bites an ELIGIBLE, still-unclaimed credit as its 30-November window closes.
            var entries = await _context.ItcRegisterEntries.AsNoTracking()
                .Where(e => !e.IsDeleted && e.Status == ItcStatus.Eligible && e.EligibleITC > 0
                         && e.Section16_4Deadline <= horizon)
                .OrderBy(e => e.Section16_4Deadline).ToListAsync();

            return await Decorate(entries, e => e.Section16_4Deadline, asOf, ItcReversalReason.Section16_4,
                "Claim the credit now, or it is lost.");
        }

        private async Task<List<ItcDeadlineAlert>> Decorate(
            List<ItcRegisterEntry> entries, Func<ItcRegisterEntry, DateTime> deadline,
            DateTime asOf, string kind, string action)
        {
            var headerIds = entries.Select(e => e.TaxInvoiceHeaderID).ToList();
            var headers = await _context.TaxInvoiceHeaders.AsNoTracking()
                .Where(h => headerIds.Contains(h.TaxInvoiceHeaderID))
                .Select(h => new { h.TaxInvoiceHeaderID, h.DocumentNo, h.DocumentDate }).ToListAsync();
            var byId = headers.ToDictionary(x => x.TaxInvoiceHeaderID);

            return entries.Select(e =>
            {
                DateTime d = deadline(e).Date;
                int daysLeft = (int)(d - asOf.Date).TotalDays;
                return new ItcDeadlineAlert
                {
                    ItcRegisterEntryID = e.ItcRegisterEntryID,
                    TaxInvoiceHeaderID = e.TaxInvoiceHeaderID,
                    DocumentNo = byId.TryGetValue(e.TaxInvoiceHeaderID, out var h) ? h.DocumentNo : string.Empty,
                    DocumentDate = byId.TryGetValue(e.TaxInvoiceHeaderID, out var h2) ? h2.DocumentDate : default,
                    SupplierName = e.SupplierName,
                    EligibleITC = e.EligibleITC,
                    DeadlineKind = kind,
                    DeadlineDate = d,
                    DaysLeft = daysLeft,
                    Passed = daysLeft < 0,
                    Action = action,
                };
            }).ToList();
        }
    }
}
