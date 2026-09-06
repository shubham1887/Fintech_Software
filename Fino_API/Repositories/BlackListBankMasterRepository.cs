using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class BlackListBankMasterRepository : IBlackListBankMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<BlackListBankMaster>> _listCache;
        public BlackListBankMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<BlackListBankMaster>>(); }
        public async Task<List<BlackListBankMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllBlackListBankMasters, () => _context.BlackListBankMasters.AsNoTracking().ToList()) ?? new List<BlackListBankMaster>();
        public async Task<BlackListBankMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.BlackListBankMasterID == id); }

        public async Task<int> Insert(BlackListBankMaster entity) { await _context.BlackListBankMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBlackListBankMasters); return result; }
        public async Task<int> Update(BlackListBankMaster entity) { _context.BlackListBankMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBlackListBankMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.BlackListBankMasters.FindAsync(id); if (entity == null) return 0; _context.BlackListBankMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBlackListBankMasters); return result; }
    }
}
