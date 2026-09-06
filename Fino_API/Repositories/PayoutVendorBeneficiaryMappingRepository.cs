using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PayoutVendorBeneficiaryMappingRepository : IPayoutVendorBeneficiaryMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PayoutVendorBeneficiaryMapping>> _listCache;
        public PayoutVendorBeneficiaryMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PayoutVendorBeneficiaryMapping>>(); }
        public async Task<List<PayoutVendorBeneficiaryMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPayoutVendorBeneficiaryMappings, () => _context.PayoutVendorBeneficiaryMappings.AsNoTracking().ToList()) ?? new List<PayoutVendorBeneficiaryMapping>();
        public async Task<PayoutVendorBeneficiaryMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PayoutVendorBeneficiaryMappingID == id); }

        public async Task<int> Insert(PayoutVendorBeneficiaryMapping entity) { await _context.PayoutVendorBeneficiaryMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPayoutVendorBeneficiaryMappings); return result; }
        public async Task<int> Update(PayoutVendorBeneficiaryMapping entity) { _context.PayoutVendorBeneficiaryMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPayoutVendorBeneficiaryMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PayoutVendorBeneficiaryMappings.FindAsync(id); if (entity == null) return 0; _context.PayoutVendorBeneficiaryMappings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPayoutVendorBeneficiaryMappings); return result; }
    }
}
