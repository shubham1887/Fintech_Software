using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    /// <summary>
    /// Append-only store for commission-rate changes. There is deliberately no Update and no
    /// Delete: "Audit logs must never be deleted" is a requirement of the delegation model, and the
    /// cheapest way to keep it is to give the rest of the codebase no way to try.
    /// </summary>
    public interface ISlabCommissionAuditRepository
    {
        Task<int> InsertRange(IEnumerable<SlabCommissionAudit> entities);

        /// <summary>
        /// Filtered, paged history, newest first, plus the total the filter matched.
        /// <paramref name="ownerUserIds"/> null = unrestricted (the platform operator); otherwise
        /// the rows are clipped to slabs owned by those users, which is how a distributor sees its
        /// own branch's rate history and no one else's.
        /// </summary>
        Task<(int count, List<SlabCommissionAudit> list)> Search(
            int slabMasterId, int serviceId, DateTime? fromDate, DateTime? toDate,
            IEnumerable<int>? ownerUserIds, int skip, int take);
    }
}
