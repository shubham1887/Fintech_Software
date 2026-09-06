using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class InvoiceSettingRepository : IInvoiceSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<InvoiceSetting>> _listCache;

        public InvoiceSettingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<InvoiceSetting>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<InvoiceSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllInvoiceSettings, () =>
                _context.InvoiceSettings.AsNoTracking().ToList()
            ) ?? new List<InvoiceSetting>();
        }

        public async Task<InvoiceSetting?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.InvoiceSettingID == id);
        }

        public async Task<int> Insert(InvoiceSetting entity)
        {
            await _context.InvoiceSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllInvoiceSettings);
            return result;
        }

        public async Task<int> Update(InvoiceSetting entity)
        {
            _context.InvoiceSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllInvoiceSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.InvoiceSettings.FindAsync(id);
            if (entity == null) return 0;
            _context.InvoiceSettings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllInvoiceSettings);
            return result;
        }
    }
}
