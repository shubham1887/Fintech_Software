using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ReferralRegistrationRepository : IReferralRegistrationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ReferralRegistration>> _listCache;
        public ReferralRegistrationRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ReferralRegistration>>(); }
        public async Task<List<ReferralRegistration>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllReferralRegistrations, () => _context.ReferralRegistrations.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ReferralRegistration>();
        public async Task<ReferralRegistration?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ReferralRegistrationID == id); }
        public async Task<List<ReferralRegistration>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(ReferralRegistration entity) { await _context.ReferralRegistrations.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllReferralRegistrations); return result; }
        public async Task<int> Update(ReferralRegistration entity) { _context.ReferralRegistrations.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllReferralRegistrations); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ReferralRegistrations.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllReferralRegistrations); return result; }
    }
}
