using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SenderOTPRepository : ISenderOTPRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SenderOTP>> _listCache;
        public SenderOTPRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SenderOTP>>(); }
        public async Task<List<SenderOTP>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSenderOTPs, () => _context.SenderOTPs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<SenderOTP>();
        public async Task<SenderOTP?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SenderOTPID == id); }
        public async Task<List<SenderOTP>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(SenderOTP entity) { await _context.SenderOTPs.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSenderOTPs); return result; }
        public async Task<int> Update(SenderOTP entity) { _context.SenderOTPs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSenderOTPs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SenderOTPs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSenderOTPs); return result; }
    }
}
