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
            DateTime? fromDate, DateTime? toDate, int skip, int take)
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

            int count = await query.CountAsync();
            var list = await query
                .OrderByDescending(x => x.DateTime)
                .Skip(skip)
                .Take(take > 0 ? take : 50)
                .ToListAsync();
            return (count, list);
        }

        public async Task<TransactionMaster?> GetByID(int id)
        {
            return await _context.TransactionMasters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TransactionMasterID == id && !x.IsDeleted);
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
        {
            DateTime cutoff = DateTime.Now.AddSeconds(-Math.Max(minAgeSeconds, 0));

            // ServiceMaster rows are admin-created here, so ids are NOT the fixed FinovativeAPI ids
            // (there 4=AEPS; in this DB 4=Prepaid Recharge). Resolve the recharge family the same
            // way the Pending Recharge report does — via ServiceKinds — instead of hardcoded ids.
            var services = await _context.ServiceMasters.AsNoTracking().ToListAsync();
            var rechargeIds = ServiceKinds.IdsOf(services, ServiceKinds.Recharge);
            if (rechargeIds.Count == 0) return new List<TransactionMaster>();

            return await _context.TransactionMasters.AsNoTracking()
                .Where(t => !t.IsDeleted
                    && (t.Status == "PENDING" || t.Status == "PROCESS")
                    && t.ProviderID > 0
                    && t.Amount >= 1
                    && t.DateTime <= cutoff
                    && rechargeIds.Contains(t.ServiceID))
                .OrderBy(t => t.DateTime)
                .Take(take > 0 ? take : 50)
                .ToListAsync();
        }
    }
}
