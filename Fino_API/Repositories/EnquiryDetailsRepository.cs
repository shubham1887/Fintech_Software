using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class EnquiryDetailsRepository : IEnquiryDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<EnquiryDetails>> _listCache;
        public EnquiryDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<EnquiryDetails>>(); }
        public async Task<List<EnquiryDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllEnquiryDetails, () => _context.EnquiryDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<EnquiryDetails>();
        public async Task<EnquiryDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.EnquiryDetailsID == id); }

        public async Task<int> Insert(EnquiryDetails entity) { await _context.EnquiryDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEnquiryDetails); return result; }
        public async Task<int> Update(EnquiryDetails entity) { _context.EnquiryDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEnquiryDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.EnquiryDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEnquiryDetails); return result; }
    }
}
