using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class PaymentGatewayRepository : IPaymentGatewayRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PaymentGateway>> _listCache;
        public PaymentGatewayRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PaymentGateway>>(); }
        public async Task<List<PaymentGateway>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPaymentGateways, () => _context.PaymentGateways.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<PaymentGateway>();
        public async Task<PaymentGateway?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PaymentGatewayID == id); }
        public async Task<List<PaymentGateway>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(PaymentGateway entity) { await _context.PaymentGateways.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGateways); return result; }
        public async Task<int> Update(PaymentGateway entity) { _context.PaymentGateways.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGateways); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PaymentGateways.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGateways); return result; }
    }
}
