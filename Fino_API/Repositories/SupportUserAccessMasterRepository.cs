using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SupportUserAccessMasterRepository : ISupportUserAccessMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SupportUserAccessMaster>> _listCache;
        public SupportUserAccessMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SupportUserAccessMaster>>(); }
        public async Task<List<SupportUserAccessMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSupportUserAccessMasters, () => _context.SupportUserAccessMasters.AsNoTracking().ToList()) ?? new List<SupportUserAccessMaster>();
        public async Task<SupportUserAccessMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SupportUserAccessMasterID == id); }
        public async Task<List<SupportUserAccessMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(SupportUserAccessMaster entity) { await _context.SupportUserAccessMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSupportUserAccessMasters); return result; }
        public async Task<int> Update(SupportUserAccessMaster entity) { _context.SupportUserAccessMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSupportUserAccessMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SupportUserAccessMasters.FindAsync(id); if (entity == null) return 0; _context.SupportUserAccessMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSupportUserAccessMasters); return result; }
    }
}
