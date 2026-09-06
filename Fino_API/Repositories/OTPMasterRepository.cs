using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OTPMasterRepository : IOTPMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OTPMaster>> _listCache;

        public OTPMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<OTPMaster>>();
        }

        public async Task<List<OTPMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllOTPMasters, () =>
                _context.OTPMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()
            ) ?? new List<OTPMaster>();
        }

        public async Task<List<OTPMaster>> GetByUserID(int userID)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userID).ToList();
        }

        public async Task<OTPMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.OTPMasterID == id);
        }

        public async Task<int> Insert(OTPMaster entity)
        {
            await _context.OTPMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOTPMasters);
            return result;
        }

        public async Task<int> Update(OTPMaster entity)
        {
            _context.OTPMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOTPMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.OTPMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOTPMasters);
            return result;
        }

        public async Task<OTPMaster?> GetActiveByUserIdAndType(int userId, string type)
        {
            var list = await GetByUserID(userId);
            return list.Where(x => x.Type == type && !x.IsVerified)
                       .OrderByDescending(x => x.OTPMasterID)
                       .FirstOrDefault();
        }

        // Mobile-scoped lookup for flows where no user exists yet (public sign-up). Returns the
        // most recently issued, still-unverified OTP for that mobile + type.
        public async Task<OTPMaster?> GetActiveByMobileAndType(string mobile, string type)
        {
            var list = await GetAll();
            return list.Where(x => x.MobileNumber == mobile && x.Type == type && !x.IsVerified)
                       .OrderByDescending(x => x.OTPMasterID)
                       .FirstOrDefault();
        }
    }
}
