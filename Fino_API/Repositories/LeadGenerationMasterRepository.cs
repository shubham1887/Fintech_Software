using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class LeadGenerationMasterRepository : ILeadGenerationMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LeadGenerationMaster>> _listCache;
        public LeadGenerationMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LeadGenerationMaster>>(); }
        public async Task<List<LeadGenerationMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLeadGenerationMasters, () => _context.LeadGenerationMasters.AsNoTracking().ToList()) ?? new List<LeadGenerationMaster>();
        public async Task<LeadGenerationMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LeadGenerationMasterID == id); }

        public async Task<int> Insert(LeadGenerationMaster entity) { await _context.LeadGenerationMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLeadGenerationMasters); return result; }
        public async Task<int> Update(LeadGenerationMaster entity) { _context.LeadGenerationMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLeadGenerationMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LeadGenerationMasters.FindAsync(id); if (entity == null) return 0; _context.LeadGenerationMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLeadGenerationMasters); return result; }
    }
}
