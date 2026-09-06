using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CompanyMasterRepository : ICompanyMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CompanyMaster>> _listCache;

        public CompanyMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<CompanyMaster>>();
        }

        public async Task<List<CompanyMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllCompanyMasters, () =>
                _context.CompanyMasters.AsNoTracking().ToList()
            ) ?? new List<CompanyMaster>();
        }

        public async Task<CompanyMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.CompanyMasterID == id);
        }

        public async Task<int> Insert(CompanyMaster entity)
        {
            await _context.CompanyMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCompanyMasters);
            return result;
        }

        public async Task<int> Update(CompanyMaster entity)
        {
            _context.CompanyMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCompanyMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.CompanyMasters.FindAsync(id);
            if (entity == null) return 0;
            _context.CompanyMasters.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCompanyMasters);
            return result;
        }
    }
}
