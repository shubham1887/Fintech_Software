using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SlabCommissionAuditRepository : ISlabCommissionAuditRepository
    {
        private readonly ApplicationDbContext _context;

        public SlabCommissionAuditRepository(ApplicationDbContext context) => _context = context;

        public async Task<int> InsertRange(IEnumerable<SlabCommissionAudit> entities)
        {
            var list = entities.ToList();
            if (list.Count == 0) return 0;
            await _context.SlabCommissionAudits.AddRangeAsync(list);
            return await _context.SaveChangesAsync();
        }

        public async Task<(int count, List<SlabCommissionAudit> list)> Search(
            int slabMasterId, int serviceId, DateTime? fromDate, DateTime? toDate,
            IEnumerable<int>? ownerUserIds, int skip, int take)
        {
            var query = _context.SlabCommissionAudits.AsNoTracking().AsQueryable();

            if (slabMasterId > 0) query = query.Where(x => x.SlabMasterID == slabMasterId);
            if (serviceId > 0) query = query.Where(x => x.ServiceID == serviceId);
            if (ownerUserIds != null)
            {
                var ids = ownerUserIds.ToList();
                query = query.Where(x => x.OwnerUserID != null && ids.Contains(x.OwnerUserID.Value));
            }
            if (fromDate.HasValue) query = query.Where(x => x.DateTime >= fromDate.Value.Date);
            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1);
                query = query.Where(x => x.DateTime < end);
            }

            int count = await query.CountAsync();
            var list = await query
                .OrderByDescending(x => x.DateTime)
                .ThenByDescending(x => x.SlabCommissionAuditID)
                .Skip(Math.Max(skip, 0))
                .Take(take > 0 ? take : 25)
                .ToListAsync();

            return (count, list);
        }
    }
}
