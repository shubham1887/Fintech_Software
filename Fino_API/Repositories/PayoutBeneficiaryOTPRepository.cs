using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PayoutBeneficiaryOTPRepository : IPayoutBeneficiaryOTPRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PayoutBeneficiaryOTP>> _listCache;
        public PayoutBeneficiaryOTPRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PayoutBeneficiaryOTP>>(); }
        public async Task<List<PayoutBeneficiaryOTP>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPayoutBeneficiaryOTPs, () => _context.PayoutBeneficiaryOTPs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<PayoutBeneficiaryOTP>();
        public async Task<PayoutBeneficiaryOTP?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PayoutBeneficiaryOTPID == id); }
        public async Task<List<PayoutBeneficiaryOTP>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(PayoutBeneficiaryOTP entity) { await _context.PayoutBeneficiaryOTPs.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPayoutBeneficiaryOTPs); return result; }
        public async Task<int> Update(PayoutBeneficiaryOTP entity) { _context.PayoutBeneficiaryOTPs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPayoutBeneficiaryOTPs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PayoutBeneficiaryOTPs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPayoutBeneficiaryOTPs); return result; }
    }
}
