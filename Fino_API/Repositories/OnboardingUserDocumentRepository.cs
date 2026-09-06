using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class OnboardingUserDocumentRepository : IOnboardingUserDocumentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OnboardingUserDocument>> _listCache;
        public OnboardingUserDocumentRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OnboardingUserDocument>>(); }
        public async Task<List<OnboardingUserDocument>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOnboardingUserDocuments, () => _context.OnboardingUserDocuments.AsNoTracking().ToList()) ?? new List<OnboardingUserDocument>();
        public async Task<OnboardingUserDocument?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OnboardingUserDocumentID == id); }

        public async Task<int> Insert(OnboardingUserDocument entity) { await _context.OnboardingUserDocuments.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOnboardingUserDocuments); return result; }
        public async Task<int> Update(OnboardingUserDocument entity) { _context.OnboardingUserDocuments.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOnboardingUserDocuments); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OnboardingUserDocuments.FindAsync(id); if (entity == null) return 0; _context.OnboardingUserDocuments.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOnboardingUserDocuments); return result; }
    }
}
