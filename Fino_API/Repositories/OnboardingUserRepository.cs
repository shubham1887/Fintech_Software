using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class OnboardingUserRepository : IOnboardingUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OnboardingUser>> _listCache;
        public OnboardingUserRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OnboardingUser>>(); }
        public async Task<List<OnboardingUser>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOnboardingUsers, () => _context.OnboardingUsers.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<OnboardingUser>();
        public async Task<OnboardingUser?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OnboardingUserID == id); }

        public async Task<int> Insert(OnboardingUser entity) { await _context.OnboardingUsers.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOnboardingUsers); return result; }
        public async Task<int> Update(OnboardingUser entity) { _context.OnboardingUsers.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOnboardingUsers); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OnboardingUsers.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOnboardingUsers); return result; }
    }
}
