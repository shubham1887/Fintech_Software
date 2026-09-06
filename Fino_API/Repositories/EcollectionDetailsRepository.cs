using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class EcollectionDetailsRepository : IEcollectionDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<EcollectionDetails>> _listCache;
        public EcollectionDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<EcollectionDetails>>(); }
        public async Task<List<EcollectionDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllEcollectionDetails, () => _context.EcollectionDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<EcollectionDetails>();
        public async Task<EcollectionDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.EcollectionDetailsID == id); }
        public async Task<List<EcollectionDetails>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(EcollectionDetails entity) { await _context.EcollectionDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEcollectionDetails); return result; }
        public async Task<int> Update(EcollectionDetails entity) { _context.EcollectionDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEcollectionDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.EcollectionDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEcollectionDetails); return result; }
    }
}
