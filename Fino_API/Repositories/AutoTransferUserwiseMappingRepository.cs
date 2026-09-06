using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class AutoTransferUserwiseMappingRepository : IAutoTransferUserwiseMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<AutoTransferUserwiseMapping>> _listCache;
        public AutoTransferUserwiseMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<AutoTransferUserwiseMapping>>(); }
        public async Task<List<AutoTransferUserwiseMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllAutoTransferUserwiseMappings, () => _context.AutoTransferUserwiseMappings.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<AutoTransferUserwiseMapping>();
        public async Task<AutoTransferUserwiseMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.AutoTransferUserwiseMappingID == id); }
        public async Task<List<AutoTransferUserwiseMapping>> GetByUserID(int userID) { var list = await GetAll(); return list.Where(x => x.UserID == userID).ToList(); }
        public async Task<int> Insert(AutoTransferUserwiseMapping entity) { await _context.AutoTransferUserwiseMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAutoTransferUserwiseMappings); return result; }
        public async Task<int> Update(AutoTransferUserwiseMapping entity) { _context.AutoTransferUserwiseMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAutoTransferUserwiseMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.AutoTransferUserwiseMappings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAutoTransferUserwiseMappings); return result; }
    }
}
