using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UPIVendorBeneficiaryMappingRepository : IUPIVendorBeneficiaryMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UPIVendorBeneficiaryMapping>> _listCache;
        public UPIVendorBeneficiaryMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UPIVendorBeneficiaryMapping>>(); }
        public async Task<List<UPIVendorBeneficiaryMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUPIVendorBeneficiaryMappings, () => _context.UPIVendorBeneficiaryMappings.AsNoTracking().ToList()) ?? new List<UPIVendorBeneficiaryMapping>();
        public async Task<UPIVendorBeneficiaryMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UPIVendorBeneficiaryMappingID == id); }

        public async Task<int> Insert(UPIVendorBeneficiaryMapping entity) { await _context.UPIVendorBeneficiaryMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUPIVendorBeneficiaryMappings); return result; }
        public async Task<int> Update(UPIVendorBeneficiaryMapping entity) { _context.UPIVendorBeneficiaryMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUPIVendorBeneficiaryMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UPIVendorBeneficiaryMappings.FindAsync(id); if (entity == null) return 0; _context.UPIVendorBeneficiaryMappings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUPIVendorBeneficiaryMappings); return result; }
    }
}
