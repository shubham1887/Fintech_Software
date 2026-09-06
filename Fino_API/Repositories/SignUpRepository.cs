using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SignUpRepository : ISignUpRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SignUp>> _listCache;
        public SignUpRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SignUp>>(); }
        public async Task<List<SignUp>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSignUps, () => _context.SignUps.AsNoTracking().ToList()) ?? new List<SignUp>();
        public async Task<SignUp?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SignUpID == id); }

        public async Task<int> Insert(SignUp entity) { await _context.SignUps.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSignUps); return result; }
        public async Task<int> Update(SignUp entity) { _context.SignUps.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSignUps); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SignUps.FindAsync(id); if (entity == null) return 0; _context.SignUps.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSignUps); return result; }
    }
}
