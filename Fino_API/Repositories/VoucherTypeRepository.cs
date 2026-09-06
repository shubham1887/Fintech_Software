using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class VoucherTypeRepository : IVoucherTypeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<VoucherType>> _listCache;

        public VoucherTypeRepository(ApplicationDbContext context, MemoryCacheProcessing<List<VoucherType>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<VoucherType>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllVoucherTypes, () =>
                _context.VoucherTypes.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<VoucherType>();
        }

        public async Task<VoucherType?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.VoucherTypeID == id);
        }

        public async Task<int> Insert(VoucherType entity)
        {
            await _context.VoucherTypes.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherTypes);
            return result;
        }

        public async Task<int> Update(VoucherType entity)
        {
            _context.VoucherTypes.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherTypes);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.VoucherTypes.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVoucherTypes);
            return result;
        }
    }
}
