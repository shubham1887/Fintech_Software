using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PaymentGatewayLogRepository : IPaymentGatewayLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PaymentGatewayLog>> _listCache;
        public PaymentGatewayLogRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PaymentGatewayLog>>(); }
        public async Task<List<PaymentGatewayLog>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPaymentGatewayLogs, () => _context.PaymentGatewayLogs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<PaymentGatewayLog>();
        public async Task<PaymentGatewayLog?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ID == id); }
        public async Task<List<PaymentGatewayLog>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(PaymentGatewayLog entity) { await _context.PaymentGatewayLogs.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGatewayLogs); return result; }
        public async Task<int> Update(PaymentGatewayLog entity) { _context.PaymentGatewayLogs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGatewayLogs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PaymentGatewayLogs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGatewayLogs); return result; }
    }
}
