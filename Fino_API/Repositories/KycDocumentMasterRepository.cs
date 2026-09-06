using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class KycDocumentMasterRepository : IKycDocumentMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<KycDocumentMaster>> _listCache;

        public KycDocumentMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<KycDocumentMaster>>();
        }

        public async Task<List<KycDocumentMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllKycDocumentMasters, () =>
                _context.KycDocumentMasters.AsNoTracking().ToList()
            ) ?? new List<KycDocumentMaster>();
        }

        public async Task<KycDocumentMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.KycDocumentMasterID == id);
        }

        public async Task<int> Insert(KycDocumentMaster entity)
        {
            await _context.KycDocumentMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllKycDocumentMasters);
            return result;
        }

        public async Task<int> Update(KycDocumentMaster entity)
        {
            _context.KycDocumentMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllKycDocumentMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.KycDocumentMasters.FindAsync(id);
            if (entity == null) return 0;
            _context.KycDocumentMasters.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllKycDocumentMasters);
            return result;
        }
    }
}
