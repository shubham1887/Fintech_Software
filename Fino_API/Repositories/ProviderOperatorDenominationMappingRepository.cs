using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderOperatorDenominationMappingRepository : IProviderOperatorDenominationMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderOperatorDenominationMapping>> _listCache;
        public ProviderOperatorDenominationMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ProviderOperatorDenominationMapping>>(); }
        public async Task<List<ProviderOperatorDenominationMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllProviderOperatorDenominationMappings, () => _context.ProviderOperatorDenominationMappings.AsNoTracking().ToList()) ?? new List<ProviderOperatorDenominationMapping>();
        public async Task<ProviderOperatorDenominationMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ProviderOperatorDenominationMappingID == id); }
        public async Task<List<ProviderOperatorDenominationMapping>> GetByProviderID(int providerid) { var list = await GetAll(); return list.Where(x => x.ProviderID == providerid).ToList(); }
        public async Task<List<ProviderOperatorDenominationMapping>> GetByOperatorID(int operatorid) { var list = await GetAll(); return list.Where(x => x.OperatorID == operatorid).ToList(); }
        public async Task<int> Insert(ProviderOperatorDenominationMapping entity) { await _context.ProviderOperatorDenominationMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorDenominationMappings); return result; }
        public async Task<int> Update(ProviderOperatorDenominationMapping entity) { _context.ProviderOperatorDenominationMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorDenominationMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ProviderOperatorDenominationMappings.FindAsync(id); if (entity == null) return 0; _context.ProviderOperatorDenominationMappings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorDenominationMappings); return result; }
    }
}
