using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class VoucherMasterRepository : IVoucherMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<VoucherMaster>> _listCache;

        public VoucherMasterRepository(ApplicationDbContext context, MemoryCacheProcessing<List<VoucherMaster>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<VoucherMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllVoucherMasters, () =>
                _context.VoucherMasters.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.VoucherType)
                    .ToList()
            ) ?? new List<VoucherMaster>();
        }

        public async Task<List<VoucherMaster>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<VoucherMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.VoucherMasterID == id);
        }

        public async Task<int> Insert(VoucherMaster entity)
        {
            await _context.VoucherMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherMasters);
            return result;
        }

        public async Task<int> Update(VoucherMaster entity)
        {
            _context.VoucherMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.VoucherMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherMasters);
            return result;
        }
    }
}
