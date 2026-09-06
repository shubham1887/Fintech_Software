using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class FailedSignUpDataRepository : IFailedSignUpDataRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<FailedSignUpData>> _listCache;
        public FailedSignUpDataRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<FailedSignUpData>>(); }
        public async Task<List<FailedSignUpData>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllFailedSignUpDatas, () => _context.FailedSignUpDatas.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<FailedSignUpData>();
        public async Task<FailedSignUpData?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.FailedSignUpDataID == id); }

        public async Task<int> Insert(FailedSignUpData entity) { await _context.FailedSignUpDatas.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllFailedSignUpDatas); return result; }
        public async Task<int> Update(FailedSignUpData entity) { _context.FailedSignUpDatas.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllFailedSignUpDatas); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.FailedSignUpDatas.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllFailedSignUpDatas); return result; }
    }
}
