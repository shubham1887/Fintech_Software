using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class KYCApprovedDetailsRepository : IKYCApprovedDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<KYCApprovedDetails>> _listCache;
        public KYCApprovedDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<KYCApprovedDetails>>(); }
        public async Task<List<KYCApprovedDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllKYCApprovedDetails, () => _context.KYCApprovedDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<KYCApprovedDetails>();
        public async Task<KYCApprovedDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.KYCApprovedDetailsID == id); }
        public async Task<List<KYCApprovedDetails>> GetByUserID(int userID) { var list = await GetAll(); return list.Where(x => x.UserID == userID).ToList(); }
        public async Task<int> Insert(KYCApprovedDetails entity) { await _context.KYCApprovedDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllKYCApprovedDetails); return result; }
        public async Task<int> Update(KYCApprovedDetails entity) { _context.KYCApprovedDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllKYCApprovedDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.KYCApprovedDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllKYCApprovedDetails); return result; }
    }
}
