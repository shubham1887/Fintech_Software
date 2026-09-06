using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class LoanTypeMasterRepository : ILoanTypeMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LoanTypeMaster>> _listCache;
        public LoanTypeMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LoanTypeMaster>>(); }
        public async Task<List<LoanTypeMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLoanTypeMasters, () => _context.LoanTypeMasters.AsNoTracking().ToList()) ?? new List<LoanTypeMaster>();
        public async Task<LoanTypeMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LoanTypeMasterID == id); }

        public async Task<int> Insert(LoanTypeMaster entity) { await _context.LoanTypeMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLoanTypeMasters); return result; }
        public async Task<int> Update(LoanTypeMaster entity) { _context.LoanTypeMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLoanTypeMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LoanTypeMasters.FindAsync(id); if (entity == null) return 0; _context.LoanTypeMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLoanTypeMasters); return result; }
    }
}
