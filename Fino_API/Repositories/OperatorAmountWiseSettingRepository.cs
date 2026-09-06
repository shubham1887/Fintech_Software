using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OperatorAmountWiseSettingRepository : IOperatorAmountWiseSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperatorAmountWiseSetting>> _listCache;
        public OperatorAmountWiseSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OperatorAmountWiseSetting>>(); }
        public async Task<List<OperatorAmountWiseSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOperatorAmountWiseSettings, () => _context.OperatorAmountWiseSettings.AsNoTracking().ToList()) ?? new List<OperatorAmountWiseSetting>();
        public async Task<OperatorAmountWiseSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OperatorAmountWiseSettingID == id); }
        public async Task<List<OperatorAmountWiseSetting>> GetByOperatorID(int operatorID) { var list = await GetAll(); return list.Where(x => x.OperatorID == operatorID).ToList(); }
        public async Task<int> Insert(OperatorAmountWiseSetting entity) { await _context.OperatorAmountWiseSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorAmountWiseSettings); return result; }
        public async Task<int> Update(OperatorAmountWiseSetting entity) { _context.OperatorAmountWiseSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorAmountWiseSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OperatorAmountWiseSettings.FindAsync(id); if (entity == null) return 0; _context.OperatorAmountWiseSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorAmountWiseSettings); return result; }
    }
}
