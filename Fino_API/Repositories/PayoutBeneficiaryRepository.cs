using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PayoutBeneficiaryRepository : IPayoutBeneficiaryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PayoutBeneficiary>> _listCache;

        public PayoutBeneficiaryRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<PayoutBeneficiary>>();
        }

        public async Task<List<PayoutBeneficiary>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllPayoutBeneficiaries, () =>
                _context.PayoutBeneficiaries.AsNoTracking().Where(x => !x.IsDeleted).ToList()
            ) ?? new List<PayoutBeneficiary>();
        }

        public async Task<PayoutBeneficiary?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.PayoutBeneficaryID == id);
        }

        public async Task<List<PayoutBeneficiary>> GetByUserID(int userID)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userID).ToList();
        }

        public async Task<int> Insert(PayoutBeneficiary entity)
        {
            await _context.PayoutBeneficiaries.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPayoutBeneficiaries);
            return result;
        }

        public async Task<int> Update(PayoutBeneficiary entity)
        {
            _context.PayoutBeneficiaries.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPayoutBeneficiaries);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.PayoutBeneficiaries.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPayoutBeneficiaries);
            return result;
        }
    }
}
