using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class BeneficiaryRepository : IBeneficiaryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<Beneficiary>> _listCache;
        public BeneficiaryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<Beneficiary>>(); }
        public async Task<List<Beneficiary>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllBeneficiarys, () => _context.Beneficiarys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<Beneficiary>();
        public async Task<Beneficiary?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.BeneficiaryID == id); }
        public async Task<List<Beneficiary>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(Beneficiary entity) { await _context.Beneficiarys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBeneficiarys); return result; }
        public async Task<int> Update(Beneficiary entity) { _context.Beneficiarys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBeneficiarys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.Beneficiarys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBeneficiarys); return result; }
    }
}
