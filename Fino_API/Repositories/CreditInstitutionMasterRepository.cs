using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class CreditInstitutionMasterRepository : ICreditInstitutionMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CreditInstitutionMaster>> _listCache;
        public CreditInstitutionMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CreditInstitutionMaster>>(); }
        public async Task<List<CreditInstitutionMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCreditInstitutionMasters, () => _context.CreditInstitutionMasters.AsNoTracking().ToList()) ?? new List<CreditInstitutionMaster>();
        public async Task<CreditInstitutionMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CreditInstitutionMasterID == id); }

        public async Task<int> Insert(CreditInstitutionMaster entity) { await _context.CreditInstitutionMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCreditInstitutionMasters); return result; }
        public async Task<int> Update(CreditInstitutionMaster entity) { _context.CreditInstitutionMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCreditInstitutionMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CreditInstitutionMasters.FindAsync(id); if (entity == null) return 0; _context.CreditInstitutionMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCreditInstitutionMasters); return result; }
    }
}
