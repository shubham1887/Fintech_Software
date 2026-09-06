using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class AuthorizationMasterRepository : IAuthorizationMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<AuthorizationMaster>> _listCache;
        public AuthorizationMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<AuthorizationMaster>>(); }
        public async Task<List<AuthorizationMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllAuthorizationMasters, () => _context.AuthorizationMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<AuthorizationMaster>();
        public async Task<AuthorizationMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.AuthorizationMasterID == id); }

        public async Task<int> Insert(AuthorizationMaster entity) { await _context.AuthorizationMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAuthorizationMasters); return result; }
        public async Task<int> Update(AuthorizationMaster entity) { _context.AuthorizationMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAuthorizationMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.AuthorizationMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAuthorizationMasters); return result; }
    }
}
