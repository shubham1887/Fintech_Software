using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class DateFormatMasterRepository : IDateFormatMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DateFormatMaster>> _listCache;
        public DateFormatMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<DateFormatMaster>>(); }
        public async Task<List<DateFormatMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllDateFormatMasters, () => _context.DateFormatMasters.AsNoTracking().ToList()) ?? new List<DateFormatMaster>();
        public async Task<DateFormatMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.DateFormatMasterkID == id); }

        public async Task<int> Insert(DateFormatMaster entity) { await _context.DateFormatMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDateFormatMasters); return result; }
        public async Task<int> Update(DateFormatMaster entity) { _context.DateFormatMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDateFormatMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.DateFormatMasters.FindAsync(id); if (entity == null) return 0; _context.DateFormatMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDateFormatMasters); return result; }
    }
}
