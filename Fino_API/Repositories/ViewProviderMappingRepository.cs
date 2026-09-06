using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ViewProviderMappingRepository : IViewProviderMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ViewProviderMapping>> _listCache;
        public ViewProviderMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ViewProviderMapping>>(); }

        public async Task<List<ViewProviderMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllViewProviderMappings, () => _context.ViewProviderMappings.AsNoTracking().ToList()) ?? new List<ViewProviderMapping>();

        public async Task<ViewProviderMapping?> GetByOperatorID(int operatorID) { var list = await GetAll(); return list.FirstOrDefault(x => x.OperatorID == operatorID); }

        public async Task<int> GetViewProviderID(int operatorID) { var item = await GetByOperatorID(operatorID); return item?.ProviderID ?? 0; }

        public async Task<int> Upsert(int operatorID, int providerID)
        {
            var existing = await _context.ViewProviderMappings.FirstOrDefaultAsync(x => x.OperatorID == operatorID);
            if (existing == null)
                await _context.ViewProviderMappings.AddAsync(new ViewProviderMapping { OperatorID = operatorID, ProviderID = providerID });
            else
                existing.ProviderID = providerID;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllViewProviderMappings);
            return result;
        }
    }
}
