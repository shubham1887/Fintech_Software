using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class LastChangeUserDetailsRepository : ILastChangeUserDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LastChangeUserDetails>> _listCache;
        public LastChangeUserDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LastChangeUserDetails>>(); }
        public async Task<List<LastChangeUserDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLastChangeUserDetailss, () => _context.LastChangeUserDetailss.AsNoTracking().ToList()) ?? new List<LastChangeUserDetails>();
        public async Task<LastChangeUserDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LastChangeUserDetailsID == id); }

        public async Task<int> Insert(LastChangeUserDetails entity) { await _context.LastChangeUserDetailss.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLastChangeUserDetailss); return result; }
        public async Task<int> Update(LastChangeUserDetails entity) { _context.LastChangeUserDetailss.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLastChangeUserDetailss); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LastChangeUserDetailss.FindAsync(id); if (entity == null) return 0; _context.LastChangeUserDetailss.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLastChangeUserDetailss); return result; }
    }
}
