using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.TicketMaster;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class TicketMasterRepository : ITicketMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TicketMaster>> _listCache;

        public TicketMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<TicketMaster>>();
        }

        public async Task<List<TicketMaster>> GetAllTicketMasters()
        {
            return _listCache.GetOrCreate(CacheKeys.AllTicketMasters, () =>
            {
                return _context.TicketMasters
                    .Include(t => t.TicketCategory)
                    .Include(t => t.UserMaster1)
                    .Include(t => t.UserTypeMaster)
                    .Where(t => t.IsDelete == false)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<TicketMaster>();
        }

        public async Task<TicketMaster?> GetTicketMasterById(int ticketMasterId)
        {
            var all = await GetAllTicketMasters();
            return all.FirstOrDefault(t => t.TicketMasterID == ticketMasterId);
        }

        public async Task<List<TicketMaster>> GetTicketMasters(GetTicketMasterRequest request)
        {
            var all = await GetAllTicketMasters();
            var query = all.AsQueryable();

            if (request.TicketMasterID > 0)
                query = query.Where(t => t.TicketMasterID == request.TicketMasterID);

            if (request.TicketCategoryID > 0)
                query = query.Where(t => t.TicketCategoryID == request.TicketCategoryID);

            if (request.UserID > 0)
                query = query.Where(t => t.UserID == request.UserID);

            if (request.UserTypeID > 0)
                query = query.Where(t => t.UserTyepID == request.UserTypeID);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(t => t.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            return query.ToList();
        }

        public async Task<int> Insert(TicketMaster ticketMaster)
        {
            await _context.TicketMasters.AddAsync(ticketMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<int> Update(TicketMaster ticketMaster)
        {
            _context.TicketMasters.Update(ticketMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        public async Task<int> Delete(TicketMaster ticketMaster)
        {
            _context.TicketMasters.Update(ticketMaster);   // Soft delete
            var result = await _context.SaveChangesAsync();

            if (result > 0) ClearCache();
            return result;
        }

        private void ClearCache() => _listCache.Remove(CacheKeys.AllTicketMasters);
    }
}