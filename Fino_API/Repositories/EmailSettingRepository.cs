using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class EmailSettingRepository : IEmailSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<EmailSettingMaster>> _listCache;

        public EmailSettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<EmailSettingMaster>>();
        }

        public async Task<List<EmailSettingMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllEmailSettings, () =>
                _context.EmailSettingMasters
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<EmailSettingMaster>();
        }

        public async Task<EmailSettingMaster?> GetByID(int emailSettingId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.EmailSettingMasterID == emailSettingId);
        }

        public async Task<int> Insert(EmailSettingMaster entity)
        {
            await _context.EmailSettingMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllEmailSettings);
            return result;
        }

        public async Task<int> Update(EmailSettingMaster entity)
        {
            _context.EmailSettingMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllEmailSettings);
            return result;
        }

        public async Task<int> Delete(int emailSettingId)
        {
            var entity = await _context.EmailSettingMasters.FindAsync(emailSettingId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllEmailSettings);
            return result;
        }
    }
}
