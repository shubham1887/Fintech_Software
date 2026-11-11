using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class TransactionMasterRepository : ITransactionMasterRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionMasterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Statuses were written inconsistently over time (FAILED vs FAILURE, PENDING vs PROCESS,
        // REFUND vs REFUNDED), and the UI sends "ALL" to mean "no filter" — expand to synonyms.
        private static string[]? StatusSynonyms(string status)
        {
            string s = (status ?? string.Empty).Trim().ToUpperInvariant();
            if (s.Length == 0 || s == "ALL") return null;
            return s switch
            {
                "FAILED" or "FAILURE" => new[] { "FAILED", "FAILURE" },
                "PENDING" or "PROCESS" => new[] { "PENDING", "PROCESS" },
                "REFUND" or "REFUNDED" => new[] { "REFUND", "REFUNDED" },
                _ => new[] { s },
            };
        }

        public async Task<List<TransactionMaster>> GetAll(int userId, string status, string serviceType, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
        {
            var query = _context.TransactionMasters.AsNoTracking().Where(x => !x.IsDeleted);

            if (userId > 0)
                query = query.Where(x => x.UserID == userId);

            var statuses = StatusSynonyms(status);
            if (statuses != null)
                query = query.Where(x => statuses.Contains(x.Status));

            if (fromDate.HasValue)
                query = query.Where(x => x.DateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.DateTime <= toDate.Value);

            return await query
                .OrderByDescending(x => x.DateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Filter-report query (Recharge / DMT / BBPS / Payout report screens): scoping by a set
        // of ServiceIDs plus the UI's Service/Operator dropdown filters, with a true total count.
        public async Task<(int count, List<TransactionMaster> list)> GetFiltered(
            int userId, string status, List<int>? serviceIds, int serviceId, int operatorId,
            DateTime? fromDate, DateTime? toDate, int skip, int take, TxnTextFilter? text = null)
        {
            var query = FilteredQuery(userId, status, serviceIds, serviceId, operatorId, fromDate, toDate, text);

            int count = await query.CountAsync();
            // The id breaks ties on the timestamp. Without it two rows written in the same second
            // can swap places between two reads, and an export that walks the windows would then
            // carry one of them twice and miss the other.
            var list = await query
                .OrderByDescending(x => x.DateTime)
                .ThenByDescending(x => x.TransactionMasterID)
                .Skip(skip)
                .Take(take > 0 ? take : 50)
                .ToListAsync();
            return (count, list);
        }

        /// <summary>
        /// The figures strip above a filter report: success / failed / pending money and counts,
        /// and the net commission, over EVERYTHING the filter matched rather than the page on
        /// screen. Three counts of a 400-row report cannot be read off 20 rows, and pulling all 400
        /// back to add them up in memory is the thing to avoid on a transaction table.
        ///
        /// The STATUS filter is deliberately not applied: the strip is there to say how the day
        /// went, and "0 failed" because the reader happens to be looking at the success list is a
        /// worse answer than no strip at all. Every other filter — user, service, operator, dates,
        /// search — is applied, because those DO narrow what the strip is about.
        /// </summary>
        public async Task<TxnFilterTotals> GetFilteredTotals(
            int userId, List<int>? serviceIds, int serviceId, int operatorId,
            DateTime? fromDate, DateTime? toDate, TxnTextFilter? text = null)
        {
            var query = FilteredQuery(userId, status: string.Empty, serviceIds, serviceId, operatorId,
                fromDate, toDate, text);

            // One trip: group by status in the database, add the buckets up here. The status column
            // is free text and several spellings mean one thing (FAILED/FAILURE, PENDING/PROCESS),
            // so the bucketing is done after the group rather than in it.
            var grouped = await query
                .GroupBy(x => x.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(x => x.Amount),
                    Commission = g.Sum(x => x.RetailerCommission),
                })
                .ToListAsync();

            var totals = new TxnFilterTotals();
            foreach (var g in grouped)
            {
                string s = (g.Status ?? string.Empty).Trim().ToUpperInvariant();
                switch (s)
                {
                    case "SUCCESS":
                        totals.SuccessCount += g.Count;
                        totals.SuccessAmount += g.Amount;
                        // Commission is counted on settled business only. A pending transaction has
                        // not paid anybody yet and a failed one never will, so including them would
                        // make the strip disagree with the wallet.
                        totals.Commission += g.Commission;
                        break;
                    case "FAILED":
                    case "FAILURE":
                    case "REFUND":
                    case "REFUNDED":
                        totals.FailedCount += g.Count;
                        totals.FailedAmount += g.Amount;
                        break;
                    case "PENDING":
                    case "PROCESS":
                        totals.PendingCount += g.Count;
                        totals.PendingAmount += g.Amount;
                        break;
                }
                totals.TotalCount += g.Count;
            }
            return totals;
        }

        /// <summary>
        /// The filter-report query, shared by the paged read and the totals above so the strip can
        /// never describe a different set of rows from the table under it.
        /// </summary>
        private IQueryable<TransactionMaster> FilteredQuery(
            int userId, string status, List<int>? serviceIds, int serviceId, int operatorId,
            DateTime? fromDate, DateTime? toDate, TxnTextFilter? text)
        {
            var query = _context.TransactionMasters.AsNoTracking().Where(x => !x.IsDeleted);

            if (userId > 0)
                query = query.Where(x => x.UserID == userId);

            if (serviceIds is { Count: > 0 })
                query = query.Where(x => serviceIds.Contains(x.ServiceID));

            if (serviceId > 0)
                query = query.Where(x => x.ServiceID == serviceId);

            if (operatorId > 0)
                query = query.Where(x => x.OperatorID == operatorId);

            var statuses = StatusSynonyms(status);
            if (statuses != null)
                query = query.Where(x => statuses.Contains(x.Status));

            if (fromDate.HasValue)
                query = query.Where(x => x.DateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.DateTime <= toDate.Value);

            // Text filters, applied to the QUERY so that the count below is the count of what the
            // reader is actually looking at and page 2 holds the next 20 of those rows.
            if (text != null && !text.IsEmpty)
            {
                if (text.UserIDs != null)
                {
                    // A resolved-to-nothing filter must return nothing. Passing an empty list
                    // through Contains() does exactly that; the guard is only here to say so.
                    var ids = text.UserIDs;
                    query = query.Where(x => ids.Contains(x.UserID));
                }

                if (text.TransactionIDs != null)
                {
                    var txnIds = text.TransactionIDs;
                    query = query.Where(x => txnIds.Contains(x.TransactionMasterID));
                }

                string criteriaText = (text.CriteriaText ?? string.Empty).Trim();
                if (criteriaText.Length > 0)
                {
                    switch ((text.Criteria ?? string.Empty).Trim().ToLowerInvariant())
                    {
                        case "transactionid":
                            // Our own id, or the reference we quote on the receipt. A reader with
                            // a number in front of them does not know which of the two it is.
                            if (int.TryParse(criteriaText, out int txnId))
                                query = query.Where(x => x.TransactionMasterID == txnId
                                                      || (x.ReferenceNo ?? string.Empty).Contains(criteriaText));
                            else
                                query = query.Where(x => (x.ReferenceNo ?? string.Empty).Contains(criteriaText));
                            break;
                        case "consumernumber":
                            query = query.Where(x => (x.ConsumerNumber ?? string.Empty).Contains(criteriaText));
                            break;
                        case "providerrefno":
                            query = query.Where(x => (x.ProviderRefNo ?? string.Empty).Contains(criteriaText)
                                                  || (x.ProviderRequestID ?? string.Empty).Contains(criteriaText));
                            break;
                        // "usermobile" never reaches here - the caller turns it into UserIDs
                        // above, because the mobile number is a UserMaster column. An unknown
                        // criteria is ignored rather than returning nothing, so a newer UI sending
                        // a criteria this build does not know still shows its rows.
                    }
                }

                string search = (text.Search ?? string.Empty).Trim();
                if (search.Length > 0)
                {
                    query = query.Where(x =>
                        (x.ConsumerNumber ?? string.Empty).Contains(search) ||
                        (x.ReferenceNo ?? string.Empty).Contains(search) ||
                        (x.ProviderRefNo ?? string.Empty).Contains(search) ||
                        (x.ProviderRequestID ?? string.Empty).Contains(search));
                }
            }

            return query;
        }

        public async Task<TransactionMaster?> GetByID(int id)
        {
            return await _context.TransactionMasters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TransactionMasterID == id && !x.IsDeleted);
        }

        public async Task<List<TransactionMaster>> GetByIDs(IEnumerable<int> ids)
        {
            var list = ids.Where(id => id > 0).Distinct().ToList();
            if (list.Count == 0) return new List<TransactionMaster>();

            // SQL Server's parameter ceiling is 2,100 and EF turns an IN list into one parameter
            // per id, so a long page is chunked rather than failing at the driver.
            const int chunk = 1000;
            var result = new List<TransactionMaster>(list.Count);
            for (int i = 0; i < list.Count; i += chunk)
            {
                var slice = list.Skip(i).Take(chunk).ToList();
                result.AddRange(await _context.TransactionMasters.AsNoTracking()
                    .Where(x => slice.Contains(x.TransactionMasterID) && !x.IsDeleted)
                    .ToListAsync());
            }
            return result;
        }

        public async Task<decimal> SumChargeForUser(int userId, DateTime? fromDate, DateTime? toDate, int walletId = 0)
        {
            if (userId <= 0) return 0m;

            var query = _context.TransactionMasters.AsNoTracking()
                .Where(x => !x.IsDeleted && x.UserID == userId);

            if (walletId > 0) query = query.Where(x => x.WalletID == walletId);
            if (fromDate.HasValue) query = query.Where(x => x.DateTime >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.DateTime <= toDate.Value);

            // SumAsync over an empty set returns null for a non-nullable projection, so the cast
            // is what keeps this from throwing on a user with no transactions in the period.
            return await query.SumAsync(x => (decimal?)x.Charge) ?? 0m;
        }

        public async Task<TransactionMaster?> GetByProviderRequestID(string providerRequestID)
        {
            return await _context.TransactionMasters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProviderRequestID == providerRequestID && !x.IsDeleted);
        }

        // Idempotency / status-check lookup for the B2B API. Scoped by UserID so a request id is
        // only ever unique-per-API-user (two integrators may legitimately both send "REF001").
        // Newest first: if a historical duplicate ever slipped in, the latest attempt is the one
        // the caller is asking about.
        public async Task<TransactionMaster?> GetByApiUserRequestID(int userID, string apiUserRequestID)
        {
            if (userID <= 0 || string.IsNullOrWhiteSpace(apiUserRequestID)) return null;
            string key = apiUserRequestID.Trim();
            return await _context.TransactionMasters.AsNoTracking()
                .Where(x => x.UserID == userID && x.APIUserRequestID == key && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionMasterID)
                .FirstOrDefaultAsync();
        }

        // Provider callback / status match. The value the upstream provider echoes back in its
        // callback is the request id we SENT it, which is the endpoint's TransactionIDPrefix
        // concatenated with our stored ProviderRequestID (see ProviderGatewayService). So an exact
        // match may miss when a prefix is configured — fall back to a suffix match. ProviderRequestID
        // is a 30-char GUID-derived value, so an EndsWith match is effectively unique.
        public async Task<TransactionMaster?> GetByProviderRequestIDLoose(string providerRequestID)
        {
            if (string.IsNullOrWhiteSpace(providerRequestID)) return null;
            string key = providerRequestID.Trim();
            var exact = await _context.TransactionMasters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProviderRequestID == key && !x.IsDeleted);
            if (exact != null) return exact;

            // A prefix was applied when dispatching (endpoint.TransactionIDPrefix + our id), so the
            // callback id is longer than what we stored. Our recharge ProviderRequestID is always a
            // 30-char GUID-derived value, so the last 30 chars of the callback id are our stored id.
            if (key.Length > 30)
            {
                string tail = key.Substring(key.Length - 30);
                return await _context.TransactionMasters.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ProviderRequestID == tail && !x.IsDeleted);
            }
            return null;
        }

        public async Task<int> Insert(TransactionMaster entity)
        {
            await _context.TransactionMasters.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateStatus(int transactionMasterID, string status, string txnID, string remark)
        {
            var entity = await _context.TransactionMasters.FindAsync(transactionMasterID);
            if (entity == null) return 0;

            entity.Status = status;
            if (!string.IsNullOrWhiteSpace(txnID))
                entity.ProviderRefNo = txnID;
            if (!string.IsNullOrWhiteSpace(remark))
                entity.Description = remark;
            entity.UpdatedateTime = DateTime.UtcNow;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateProviderReply(int transactionMasterID, int providerID, string referenceNo, string rawResponse, decimal? providerOpeningBalance, decimal? providerClosingBalance)
        {
            var entity = await _context.TransactionMasters.FindAsync(transactionMasterID);
            if (entity == null) return 0;

            if (providerID > 0) entity.ProviderID = providerID;
            if (!string.IsNullOrWhiteSpace(referenceNo))
            {
                entity.ReferenceNo = referenceNo;
                entity.ProviderRefNo = referenceNo;
            }
            if (!string.IsNullOrWhiteSpace(rawResponse)) entity.Response = rawResponse;
            if (providerOpeningBalance.HasValue) entity.ProviderOpeningBalance = providerOpeningBalance.Value;
            if (providerClosingBalance.HasValue) entity.ProviderClosingBalance = providerClosingBalance.Value;
            entity.UpdatedateTime = DateTime.UtcNow;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateCommissionOutcome(int transactionMasterID, decimal providerCommission, decimal adminCommission)
        {
            var entity = await _context.TransactionMasters.FindAsync(transactionMasterID);
            if (entity == null) return 0;

            entity.ProviderCommission = providerCommission;
            entity.AdminCommission = adminCommission;
            entity.AdminProfit = adminCommission;
            entity.UpdatedateTime = DateTime.UtcNow;

            return await _context.SaveChangesAsync();
        }

        // Appbar pending counts — direct DB queries (no cache, high-volume table)
        public async Task<int> GetPendingCount()
        {
            return await _context.TransactionMasters.AsNoTracking()
                .CountAsync(t => !t.IsDeleted && t.Status == "PROCESS" && t.Amount >= 1
                    && t.ServiceID != 4  // AEPS
                    && t.ServiceID != 10 // MoneyTransfer/DMT
                    && t.ServiceID != 5  // MATM
                    && t.ServiceID != 28 // Payout
                    && t.ServiceID != 17 // CashDeposit
                    && t.ServiceID != 34 // PanCard
                    && t.ServiceID != 31 // UPITransfer
                    && t.ServiceID != 16 // CreditCardPayment
                );
        }

        public async Task<int> GetPendingDMTCount()
        {
            return await _context.TransactionMasters.AsNoTracking()
                .CountAsync(t => !t.IsDeleted && t.Status == "PROCESS" && t.ServiceID == 10);
        }

        public async Task<int> GetPendingUPICount()
        {
            return await _context.TransactionMasters.AsNoTracking()
                .CountAsync(t => !t.IsDeleted && t.Status == "PROCESS" && t.ServiceID == 31);
        }

        // Pending recharge-family transactions the scheduler should re-enquire against the provider.
        // Recharge dispatch leaves provider-PENDING recharges as PENDING (legacy rows use PROCESS),
        // so both are picked up. Same service-id exclusions as GetPendingCount — AEPS/DMT/MATM/Payout/
        // CashDeposit/PanCard/UPI/CreditCard settle through their own status-check jobs. ProviderID > 0
        // means it actually reached a provider, and minAgeSeconds gives the provider time to settle
        // before we probe (mirrors the old isValidForBulkStatusCheck 90-second guard).
        public async Task<List<TransactionMaster>> GetPendingRechargeForStatusCheck(int minAgeSeconds, int take)
            => await GetPendingForStatusCheck(ServiceKinds.Recharge, minAgeSeconds, take);

        // Pending transactions of ONE service family eligible for an automated status re-check.
        // Shared by the Hangfire sweeps (recharge + DMT) so both families settle the same way.
        //
        // Eligible = still PENDING/PROCESS, already dispatched to a provider (ProviderID > 0) and at
        // least minAgeSeconds old. A HELD DMT transaction is deliberately NOT dispatched yet
        // (ProviderID stays 0), so this query skips it — that is what keeps a hold on hold.
        public async Task<List<TransactionMaster>> GetPendingForStatusCheck(string serviceKind, int minAgeSeconds, int take)
        {
            DateTime cutoff = DateTime.Now.AddSeconds(-Math.Max(minAgeSeconds, 0));

            // ServiceMaster rows are admin-created here, so ids are NOT the fixed FinovativeAPI ids
            // (there 4=AEPS; in this DB 4=Prepaid Recharge). Resolve the family the same way the
            // Pending reports do — via ServiceKinds — instead of hardcoded ids.
            var services = await _context.ServiceMasters.AsNoTracking().ToListAsync();
            var serviceIds = ServiceKinds.IdsOf(services, serviceKind);
            if (serviceIds.Count == 0) return new List<TransactionMaster>();

            return await _context.TransactionMasters.AsNoTracking()
                .Where(t => !t.IsDeleted
                    && (t.Status == "PENDING" || t.Status == "PROCESS")
                    && t.ProviderID > 0
                    && t.Amount >= 1
                    && t.DateTime <= cutoff
                    && serviceIds.Contains(t.ServiceID))
                .OrderBy(t => t.DateTime)
                .Take(take > 0 ? take : 50)
                .ToListAsync();
        }

        // DMT transactions parked by the global "Hold DMT" switch: money already debited and
        // commissions posted, but never dispatched upstream (ProviderID = 0, Description carries
        // the hold marker). Releasing them re-runs the normal DMT provider sequence.
        public async Task<List<TransactionMaster>> GetHeldDMT(int take)
        {
            var services = await _context.ServiceMasters.AsNoTracking().ToListAsync();
            var dmtIds = ServiceKinds.IdsOf(services, ServiceKinds.DMT);
            if (dmtIds.Count == 0) return new List<TransactionMaster>();

            return await _context.TransactionMasters.AsNoTracking()
                .Where(t => !t.IsDeleted
                    && (t.Status == "PENDING" || t.Status == "PROCESS")
                    && t.ProviderID == 0
                    && t.Description.Contains(TransactionHold.Marker)
                    && dmtIds.Contains(t.ServiceID))
                .OrderBy(t => t.DateTime)
                .Take(take > 0 ? take : 200)
                .ToListAsync();
        }

        // Sums what one remitter has sent since fromDate. Deliberately NOT scoped to a single API
        // user: the per-sender cap is a limit on the REMITTER, so it must hold across every outlet
        // and every channel they transact through, or the cap could be bypassed by spreading the
        // same customer's transfers over two retailers.
        //
        // Aggregated in the database rather than by loading rows, so a busy remitter costs one
        // scalar read on the hot path of every transfer.
        public async Task<decimal> GetDMTConsumedBySender(string senderMobile, DateTime fromDate)
        {
            string mobile = (senderMobile ?? string.Empty).Trim();
            if (mobile.Length == 0) return 0m;

            var services = await _context.ServiceMasters.AsNoTracking().ToListAsync();
            var dmtIds = ServiceKinds.IdsOf(services, ServiceKinds.DMT);
            if (dmtIds.Count == 0) return 0m;

            // Anything that settled as failed, refunded or reversed released the money and must not
            // count against the cap. Everything else — SUCCESS and still-in-flight alike — does.
            return await _context.TransactionMasters.AsNoTracking()
                .Where(t => !t.IsDeleted
                    && t.DMT_SenderNumber == mobile
                    && t.DateTime >= fromDate
                    && dmtIds.Contains(t.ServiceID)
                    && t.Status != "FAILED" && t.Status != "FAILURE"
                    && t.Status != "REFUND" && t.Status != "REFUNDED"
                    && t.Status != "REVERSED")
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;
        }
    }
}
