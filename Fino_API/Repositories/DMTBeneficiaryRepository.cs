using FintechApp.Processing;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class DMTBeneficiaryRepository : IDMTBeneficiaryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DMTBeneficiary>> _listCache;

        public DMTBeneficiaryRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<DMTBeneficiary>>();
        }

        public async Task<List<DMTBeneficiary>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllDMTBeneficiaries, () =>
                _context.DMTBeneficiaries.AsNoTracking().Where(x => !x.IsDeleted).ToList()
            ) ?? new List<DMTBeneficiary>();
        }

        public async Task<DMTBeneficiary?> GetByID(int id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.BeneficiaryID == id);
        }

        public async Task<List<DMTBeneficiary>> GetBySenderID(int senderId)
        {
            string cacheKey = CacheKeys.GetDMTBeneficiariesBySenderKey(senderId);
            return _listCache.GetOrCreate(cacheKey, () =>
                _context.DMTBeneficiaries.AsNoTracking()
                    .Where(x => x.SenderID == senderId && !x.IsDeleted).ToList()
            ) ?? new List<DMTBeneficiary>();
        }

        public async Task<DMTBeneficiary?> GetByAccountAndSender(string accountNo, int senderId)
        {
            var senderBeneficiaries = await GetBySenderID(senderId);
            return senderBeneficiaries.FirstOrDefault(x => x.AccountNo == accountNo);
        }

        public async Task<int> Insert(DMTBeneficiary entity)
        {
            await _context.DMTBeneficiaries.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _listCache.Remove(CacheKeys.AllDMTBeneficiaries);
                _listCache.Remove(CacheKeys.GetDMTBeneficiariesBySenderKey(entity.SenderID));
            }
            return result;
        }

        public async Task<int> Update(DMTBeneficiary entity)
        {
            _context.DMTBeneficiaries.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _listCache.Remove(CacheKeys.AllDMTBeneficiaries);
                _listCache.Remove(CacheKeys.GetDMTBeneficiariesBySenderKey(entity.SenderID));
            }
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.DMTBeneficiaries.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _listCache.Remove(CacheKeys.AllDMTBeneficiaries);
                _listCache.Remove(CacheKeys.GetDMTBeneficiariesBySenderKey(entity.SenderID));
            }
            return result;
        }
    }
}
