using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PaymentGatewaySettingRepository : IPaymentGatewaySettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PaymentGatewaySetting>> _listCache;

        public PaymentGatewaySettingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<PaymentGatewaySetting>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<PaymentGatewaySetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllPaymentGatewaySettings, () =>
                _context.PaymentGatewaySettings.AsNoTracking().ToList()
            ) ?? new List<PaymentGatewaySetting>();
        }

        public async Task<PaymentGatewaySetting?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.PaymentGatewaySettingID == id);
        }

        public async Task<int> Insert(PaymentGatewaySetting entity)
        {
            await _context.PaymentGatewaySettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGatewaySettings);
            return result;
        }

        public async Task<int> Update(PaymentGatewaySetting entity)
        {
            _context.PaymentGatewaySettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGatewaySettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.PaymentGatewaySettings.FindAsync(id);
            if (entity == null) return 0;
            _context.PaymentGatewaySettings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPaymentGatewaySettings);
            return result;
        }
    }
}
