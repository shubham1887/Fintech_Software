using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class PaySprintBankRepository : IPaySprintBankRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PaySprintBank>> _listCache;
        public PaySprintBankRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PaySprintBank>>(); }
        public async Task<List<PaySprintBank>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPaySprintBanks, () => _context.PaySprintBanks.AsNoTracking().ToList()) ?? new List<PaySprintBank>();
        public async Task<PaySprintBank?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ID == id); }

        public async Task<int> Insert(PaySprintBank entity) { await _context.PaySprintBanks.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaySprintBanks); return result; }
        public async Task<int> Update(PaySprintBank entity) { _context.PaySprintBanks.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaySprintBanks); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PaySprintBanks.FindAsync(id); if (entity == null) return 0; _context.PaySprintBanks.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaySprintBanks); return result; }
    }
}
