using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase4;
using FintechApp.Responses.Phase4;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ILedgerPostingService" />
    public class LedgerPostingService : ILedgerPostingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPeriodLockService _locks;
        private readonly Logger _logger = new();

        public LedgerPostingService(ApplicationDbContext context, IPeriodLockService locks)
        {
            _context = context;
            _locks = locks;
        }

        // ── Enqueue ────────────────────────────────────────────────────────────────────────
        public async Task<EnqueuePostingResult> Enqueue(EnqueuePostingRequest request)
        {
            string key = request.EventKey.Trim();
            if (string.IsNullOrWhiteSpace(key)) key = $"{request.EventType}:{Guid.NewGuid():N}";

            // Idempotent on the event key: the same event enqueued twice is one row.
            var existing = await _context.AcctPostingOutbox.AsNoTracking()
                .FirstOrDefaultAsync(o => o.EventKey == key && !o.IsDeleted);
            if (existing != null)
                return new EnqueuePostingResult { AcctPostingOutboxID = existing.AcctPostingOutboxID, AlreadyExisted = true };

            var payload = new JObject();
            foreach (var kv in request.Amounts) payload[kv.Key] = kv.Value;

            var row = new AcctPostingOutbox
            {
                EventType = request.EventType.Trim(),
                EventKey = key,
                EventDate = (request.EventDate ?? DateTime.Today).Date,
                PayloadJson = payload.ToString(Formatting.None),
                Narration = request.Narration.Trim(),
                Status = OutboxStatus.Pending,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };
            _context.AcctPostingOutbox.Add(row);
            await _context.SaveChangesAsync();
            return new EnqueuePostingResult { AcctPostingOutboxID = row.AcctPostingOutboxID, AlreadyExisted = false };
        }

        // ── The engine ───────────────────────────────────────────────────────────────────
        public async Task<OutboxProcessResult> ProcessOutbox(int maxItems)
        {
            var result = new OutboxProcessResult();
            int take = maxItems <= 0 ? 200 : maxItems;

            // Pending and previously-Failed rows, oldest first. Failed rows are retried because the
            // reason may have cleared (a period reopened, a rule added) — that is the whole point of
            // not failing silently: the row stays visible and keeps trying.
            var rows = await _context.AcctPostingOutbox
                .Where(o => !o.IsDeleted && (o.Status == OutboxStatus.Pending || o.Status == OutboxStatus.Failed))
                .OrderBy(o => o.AcctPostingOutboxID)
                .Take(take)
                .ToListAsync();

            // The chart and rules, read once for the batch.
            var accounts = await _context.AcctAccounts.AsNoTracking().Where(a => !a.IsDeleted && a.IsActive).ToListAsync();
            var byCode = accounts.ToDictionary(a => a.Code, a => a, StringComparer.OrdinalIgnoreCase);
            var rulesByEvent = (await _context.AcctPostingRules.AsNoTracking().Where(r => !r.IsDeleted && r.IsActive).ToListAsync())
                .GroupBy(r => r.EventType, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.LineNo).ToList(), StringComparer.OrdinalIgnoreCase);

            foreach (var o in rows)
            {
                result.Attempted++;
                o.AttemptCount++;
                o.LastAttemptOn = DateTime.UtcNow;
                o.UpdatedateTime = DateTime.UtcNow;

                try
                {
                    var outcome = await PostOne(o, byCode, rulesByEvent);
                    switch (outcome.Status)
                    {
                        case OutboxStatus.Posted:
                            o.Status = OutboxStatus.Posted;
                            o.PostedVoucherID = outcome.VoucherId;
                            o.LastError = string.Empty;
                            result.Posted++;
                            break;
                        case OutboxStatus.Skipped:
                            o.Status = OutboxStatus.Skipped;
                            o.LastError = outcome.Message;
                            result.Skipped++;
                            result.Messages.Add($"#{o.AcctPostingOutboxID} skipped: {outcome.Message}");
                            break;
                        default:
                            o.Status = OutboxStatus.Failed;
                            o.LastError = outcome.Message;
                            result.Failed++;
                            result.Messages.Add($"#{o.AcctPostingOutboxID} failed: {outcome.Message}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Never silent: the error is recorded on the row and the row stays Failed, to be
                    // retried next pass. One bad event does not stop the rest — each is saved on its own.
                    _logger.Write(ex);
                    o.Status = OutboxStatus.Failed;
                    o.LastError = Trim(ex.Message);
                    result.Failed++;
                    result.Messages.Add($"#{o.AcctPostingOutboxID} errored: {Trim(ex.Message)}");
                }

                await _context.SaveChangesAsync();
            }

            return result;
        }

        private sealed record PostOutcome(string Status, string Message, int VoucherId);

        private async Task<PostOutcome> PostOne(
            AcctPostingOutbox o,
            Dictionary<string, AcctAccount> byCode,
            Dictionary<string, List<AcctPostingRule>> rulesByEvent)
        {
            if (!rulesByEvent.TryGetValue(o.EventType, out var rules) || rules.Count == 0)
                return new PostOutcome(OutboxStatus.Skipped, $"No posting rule for event '{o.EventType}'.", 0);

            string periodKey = o.EventDate.ToString("yyyyMM");
            if (await _locks.IsLocked(periodKey))
                return new PostOutcome(OutboxStatus.Failed, $"Period {periodKey} is locked; this event cannot post into it.", 0);

            JObject payload;
            try { payload = string.IsNullOrWhiteSpace(o.PayloadJson) ? new JObject() : JObject.Parse(o.PayloadJson); }
            catch { return new PostOutcome(OutboxStatus.Failed, "The event payload is not valid JSON.", 0); }

            var lines = new List<AcctVoucherLine>();
            int lineNo = 1;
            foreach (var rule in rules)
            {
                decimal amount = (decimal?)payload[rule.AmountField] ?? 0m;
                amount = Math.Round(amount, 2);
                if (amount <= 0m) continue; // a leg with no amount (a sale with no tax) simply posts nothing

                if (!byCode.TryGetValue(rule.DebitAccountCode, out var dr))
                    return new PostOutcome(OutboxStatus.Failed, $"Debit account '{rule.DebitAccountCode}' is not in the chart.", 0);
                if (!byCode.TryGetValue(rule.CreditAccountCode, out var cr))
                    return new PostOutcome(OutboxStatus.Failed, $"Credit account '{rule.CreditAccountCode}' is not in the chart.", 0);

                string narr = string.IsNullOrWhiteSpace(rule.Narration) ? o.Narration : rule.Narration;
                lines.Add(new AcctVoucherLine { AcctAccountID = dr.AcctAccountID, AccountCode = dr.Code, LineNo = lineNo++, Debit = amount, Credit = 0m, Narration = narr });
                lines.Add(new AcctVoucherLine { AcctAccountID = cr.AcctAccountID, AccountCode = cr.Code, LineNo = lineNo++, Debit = 0m, Credit = amount, Narration = narr });
            }

            if (lines.Count == 0)
                return new PostOutcome(OutboxStatus.Skipped, "Every amount on this event was zero; nothing to post.", 0);

            int voucherId = await WriteVoucher(
                o.EventType, o.EventDate, periodKey, o.Narration, o.AcctPostingOutboxID, 0, null, lines);
            return new PostOutcome(OutboxStatus.Posted, string.Empty, voucherId);
        }

        // ── Manual journal ─────────────────────────────────────────────────────────────────
        public async Task<(PostResult? Result, string Failure)> PostManualJournal(ManualJournalRequest request)
        {
            if (request.Lines.Count < 2) return (null, "A journal needs at least two lines.");

            DateTime date = (request.VoucherDate ?? DateTime.Today).Date;
            string periodKey = date.ToString("yyyyMM");
            if (await _locks.IsLocked(periodKey)) return (null, $"Period {periodKey} is locked.");

            var accounts = await _context.AcctAccounts.AsNoTracking().Where(a => !a.IsDeleted).ToListAsync();
            var byCode = accounts.ToDictionary(a => a.Code, a => a, StringComparer.OrdinalIgnoreCase);

            decimal totalDr = 0m, totalCr = 0m;
            var lines = new List<AcctVoucherLine>();
            int lineNo = 1;
            foreach (var l in request.Lines)
            {
                if (!byCode.TryGetValue(l.AccountCode.Trim(), out var acct))
                    return (null, $"Account '{l.AccountCode}' is not in the chart.");
                decimal dr = Math.Round(Math.Max(0, l.Debit), 2), cr = Math.Round(Math.Max(0, l.Credit), 2);
                if (dr > 0 && cr > 0) return (null, $"Line {lineNo}: a line is a debit or a credit, not both.");
                if (dr == 0 && cr == 0) continue;
                totalDr += dr; totalCr += cr;
                lines.Add(new AcctVoucherLine { AcctAccountID = acct.AcctAccountID, AccountCode = acct.Code, LineNo = lineNo++, Debit = dr, Credit = cr, Narration = l.Narration.Trim() });
            }

            if (lines.Count < 2) return (null, "A journal needs at least two non-zero lines.");
            if (Math.Abs(totalDr - totalCr) > 0.01m)
                return (null, $"The journal does not balance: debits {totalDr:0.00} vs credits {totalCr:0.00}.");

            int id = await WriteVoucher(LedgerEvent.ManualJournal, date, periodKey, request.Narration.Trim(), 0, request.CreatedByUserID, null, lines);
            return (new PostResult { AcctVoucherID = id, VoucherNo = await VoucherNoOf(id), TotalDebit = totalDr, TotalCredit = totalCr }, string.Empty);
        }

        // ── Reversal ───────────────────────────────────────────────────────────────────────
        public async Task<(PostResult? Result, string Failure)> ReverseVoucher(ReverseVoucherRequest request)
        {
            var original = await _context.AcctVouchers.FirstOrDefaultAsync(v => v.AcctVoucherID == request.AcctVoucherID && !v.IsDeleted);
            if (original == null) return (null, "That voucher was not found.");
            if (original.Status == VoucherStatus.Reversed) return (null, "That voucher is already reversed.");

            DateTime date = (request.ReversalDate ?? DateTime.Today).Date;
            string periodKey = date.ToString("yyyyMM");
            if (await _locks.IsLocked(periodKey)) return (null, $"Period {periodKey} is locked; post the reversal into an open period.");

            var origLines = await _context.AcctVoucherLines.AsNoTracking()
                .Where(l => l.AcctVoucherID == original.AcctVoucherID && !l.IsDeleted).OrderBy(l => l.LineNo).ToListAsync();

            int lineNo = 1;
            var reversed = origLines.Select(l => new AcctVoucherLine
            {
                AcctAccountID = l.AcctAccountID,
                AccountCode = l.AccountCode,
                LineNo = lineNo++,
                Debit = l.Credit,   // swap
                Credit = l.Debit,
                Narration = $"Reversal: {l.Narration}",
            }).ToList();

            string narr = string.IsNullOrWhiteSpace(request.Narration) ? $"Reversal of {original.VoucherNo}" : request.Narration.Trim();
            int id = await WriteVoucher(original.VoucherType, date, periodKey, narr, 0, request.CreatedByUserID, original.AcctVoucherID, reversed);

            // Mark the original reversed and point both at each other.
            var tracked = await _context.AcctVouchers.FirstAsync(v => v.AcctVoucherID == original.AcctVoucherID);
            tracked.Status = VoucherStatus.Reversed;
            tracked.ReversalOfVoucherID = id;
            tracked.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (new PostResult { AcctVoucherID = id, VoucherNo = await VoucherNoOf(id), TotalDebit = reversed.Sum(l => l.Debit), TotalCredit = reversed.Sum(l => l.Credit) }, string.Empty);
        }

        /// <summary>Writes one balanced voucher and its lines in a transaction. The balance is
        /// asserted here as a last guard — the engine builds balanced pairs, but a manual journal
        /// comes from outside, and an unbalanced voucher must never reach the table.</summary>
        private async Task<int> WriteVoucher(
            string voucherType, DateTime date, string periodKey, string narration,
            int sourceOutboxId, int createdBy, int? reversalOf, List<AcctVoucherLine> lines)
        {
            decimal totalDr = Math.Round(lines.Sum(l => l.Debit), 2);
            decimal totalCr = Math.Round(lines.Sum(l => l.Credit), 2);
            if (Math.Abs(totalDr - totalCr) > 0.01m)
                throw new InvalidOperationException($"Refusing to post an unbalanced voucher ({totalDr:0.00} vs {totalCr:0.00}).");

            using var tx = await _context.Database.BeginTransactionAsync();
            var voucher = new AcctVoucher
            {
                VoucherNo = string.Empty, // set after we have the id, so it is unique and readable
                VoucherDate = date,
                PeriodKey = periodKey,
                VoucherType = voucherType,
                Narration = narration,
                SourceOutboxID = sourceOutboxId,
                TotalDebit = totalDr,
                TotalCredit = totalCr,
                Status = VoucherStatus.Posted,
                ReversalOfVoucherID = reversalOf,
                CreatedByUserID = createdBy,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };
            _context.AcctVouchers.Add(voucher);
            await _context.SaveChangesAsync();

            voucher.VoucherNo = $"JV{periodKey}-{voucher.AcctVoucherID}";
            foreach (var l in lines)
            {
                l.AcctVoucherID = voucher.AcctVoucherID;
                l.DateTime = DateTime.UtcNow;
                l.UpdatedateTime = DateTime.UtcNow;
                _context.AcctVoucherLines.Add(l);
            }
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return voucher.AcctVoucherID;
        }

        private async Task<string> VoucherNoOf(int id) =>
            (await _context.AcctVouchers.AsNoTracking().Where(v => v.AcctVoucherID == id).Select(v => v.VoucherNo).FirstOrDefaultAsync()) ?? string.Empty;

        // ── Reads ──────────────────────────────────────────────────────────────────────────
        public async Task<List<VoucherRow>> GetVouchers(LedgerQueryRequest query)
        {
            var q = _context.AcctVouchers.AsNoTracking().Where(v => !v.IsDeleted);
            if (!string.IsNullOrWhiteSpace(query.PeriodKey)) q = q.Where(v => v.PeriodKey == query.PeriodKey.Trim());
            if (query.FromDate.HasValue) q = q.Where(v => v.VoucherDate >= query.FromDate.Value.Date);
            if (query.ToDate.HasValue) q = q.Where(v => v.VoucherDate < query.ToDate.Value.Date.AddDays(1));

            var vouchers = await q.OrderByDescending(v => v.AcctVoucherID).Take(1000).ToListAsync();
            var ids = vouchers.Select(v => v.AcctVoucherID).ToList();
            var lines = await _context.AcctVoucherLines.AsNoTracking()
                .Where(l => ids.Contains(l.AcctVoucherID) && !l.IsDeleted).ToListAsync();
            var names = await AccountNameMap();
            var linesByVoucher = lines.GroupBy(l => l.AcctVoucherID).ToDictionary(g => g.Key, g => g.OrderBy(l => l.LineNo).ToList());

            return vouchers.Select(v => ToRow(v, linesByVoucher.TryGetValue(v.AcctVoucherID, out var ls) ? ls : new(), names)).ToList();
        }

        public async Task<VoucherRow?> GetVoucher(int voucherId)
        {
            var v = await _context.AcctVouchers.AsNoTracking().FirstOrDefaultAsync(x => x.AcctVoucherID == voucherId && !x.IsDeleted);
            if (v == null) return null;
            var lines = await _context.AcctVoucherLines.AsNoTracking()
                .Where(l => l.AcctVoucherID == voucherId && !l.IsDeleted).OrderBy(l => l.LineNo).ToListAsync();
            return ToRow(v, lines, await AccountNameMap());
        }

        public async Task<List<AcctPostingOutbox>> GetOutbox(string statusFilter)
        {
            var q = _context.AcctPostingOutbox.AsNoTracking().Where(o => !o.IsDeleted);
            if (!string.IsNullOrWhiteSpace(statusFilter)) q = q.Where(o => o.Status == statusFilter.Trim());
            return await q.OrderByDescending(o => o.AcctPostingOutboxID).Take(1000).ToListAsync();
        }

        private async Task<Dictionary<int, string>> AccountNameMap() =>
            (await _context.AcctAccounts.AsNoTracking().Select(a => new { a.AcctAccountID, a.Name }).ToListAsync())
            .ToDictionary(a => a.AcctAccountID, a => a.Name);

        private static VoucherRow ToRow(AcctVoucher v, List<AcctVoucherLine> lines, Dictionary<int, string> names) => new()
        {
            AcctVoucherID = v.AcctVoucherID,
            VoucherNo = v.VoucherNo,
            VoucherDate = v.VoucherDate,
            PeriodKey = v.PeriodKey,
            VoucherType = v.VoucherType,
            Narration = v.Narration,
            TotalDebit = v.TotalDebit,
            TotalCredit = v.TotalCredit,
            Status = v.Status,
            Lines = lines.Select(l => new VoucherLineRow
            {
                AccountCode = l.AccountCode,
                AccountName = names.TryGetValue(l.AcctAccountID, out var n) ? n : string.Empty,
                Debit = l.Debit,
                Credit = l.Credit,
                Narration = l.Narration,
            }).ToList(),
        };

        private static string Trim(string s) => string.IsNullOrEmpty(s) ? s : (s.Length > 900 ? s[..900] : s);
    }
}
