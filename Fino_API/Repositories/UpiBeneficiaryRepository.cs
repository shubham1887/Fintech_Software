using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UpiBeneficiaryRepository : IUpiBeneficiaryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UpiBeneficiary>> _listCache;
        public UpiBeneficiaryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UpiBeneficiary>>(); }
        public async Task<List<UpiBeneficiary>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUpiBeneficiarys, () => _context.UpiBeneficiarys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UpiBeneficiary>();
        public async Task<UpiBeneficiary?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UpiBeneficiaryID == id); }
        public async Task<List<UpiBeneficiary>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UpiBeneficiary entity) { await _context.UpiBeneficiarys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUpiBeneficiarys); return result; }
        public async Task<int> Update(UpiBeneficiary entity) { _context.UpiBeneficiarys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUpiBeneficiarys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UpiBeneficiarys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUpiBeneficiarys); return result; }
    }
}
