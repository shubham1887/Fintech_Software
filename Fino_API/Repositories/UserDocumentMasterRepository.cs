using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserDocumentMasterRepository : IUserDocumentMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserDocumentMaster>> _listCache;
        public UserDocumentMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserDocumentMaster>>(); }
        public async Task<List<UserDocumentMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserDocumentMasters, () => _context.UserDocumentMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserDocumentMaster>();
        public async Task<UserDocumentMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserDocumentID == id); }
        public async Task<List<UserDocumentMaster>> GetByUserID(int userID) { var list = await GetAll(); return list.Where(x => x.UserID == userID).ToList(); }
        public async Task<int> Insert(UserDocumentMaster entity) { await _context.UserDocumentMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserDocumentMasters); return result; }
        public async Task<int> Update(UserDocumentMaster entity) { _context.UserDocumentMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserDocumentMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserDocumentMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserDocumentMasters); return result; }
    }
}
