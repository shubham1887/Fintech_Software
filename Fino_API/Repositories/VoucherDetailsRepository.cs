using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class VoucherDetailsRepository : IVoucherDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<VoucherDetails>> _listCache;

        public VoucherDetailsRepository(ApplicationDbContext context, MemoryCacheProcessing<List<VoucherDetails>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<VoucherDetails>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllVoucherDetails, () =>
                _context.VoucherDetails.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .ToList()
            ) ?? new List<VoucherDetails>();
        }

        public async Task<List<VoucherDetails>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<VoucherDetails?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.VoucherDetailsID == id);
        }

        public async Task<int> Insert(VoucherDetails entity)
        {
            await _context.VoucherDetails.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherDetails);
            return result;
        }

        public async Task<int> Update(VoucherDetails entity)
        {
            _context.VoucherDetails.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherDetails);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.VoucherDetails.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherDetails);
            return result;
        }
    }
}
