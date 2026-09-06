using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class InvoiceNumberHistoryRepository : IInvoiceNumberHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<InvoiceNumberHistory>> _listCache;
        public InvoiceNumberHistoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<InvoiceNumberHistory>>(); }
        public async Task<List<InvoiceNumberHistory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllInvoiceNumberHistories, () => _context.InvoiceNumberHistories.AsNoTracking().ToList()) ?? new List<InvoiceNumberHistory>();
        public async Task<InvoiceNumberHistory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.InvoiceNumberHistoryID == id); }
        public async Task<List<InvoiceNumberHistory>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(InvoiceNumberHistory entity) { await _context.InvoiceNumberHistories.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllInvoiceNumberHistories); return result; }
        public async Task<int> Update(InvoiceNumberHistory entity) { _context.InvoiceNumberHistories.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllInvoiceNumberHistories); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.InvoiceNumberHistories.FindAsync(id); if (entity == null) return 0; _context.InvoiceNumberHistories.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllInvoiceNumberHistories); return result; }
    }
}
