using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PaymentModeMasterRepository : IPaymentModeMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PaymentModeMaster>> _listCache;
        public PaymentModeMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PaymentModeMaster>>(); }
        public async Task<List<PaymentModeMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPaymentModeMasters, () => _context.PaymentModeMasters.AsNoTracking().ToList()) ?? new List<PaymentModeMaster>();
        public async Task<PaymentModeMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PaymentModeMasterID == id); }

        public async Task<int> Insert(PaymentModeMaster entity) { await _context.PaymentModeMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentModeMasters); return result; }
        public async Task<int> Update(PaymentModeMaster entity) { _context.PaymentModeMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentModeMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PaymentModeMasters.FindAsync(id); if (entity == null) return 0; _context.PaymentModeMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentModeMasters); return result; }
    }
}
