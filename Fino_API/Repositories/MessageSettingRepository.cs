using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class MessageSettingRepository : IMessageSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<MessageSetting>> _listCache;

        public MessageSettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<MessageSetting>>();
        }

        public async Task<List<MessageSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllMessageSettings, () =>
                _context.MessageSettings
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<MessageSetting>();
        }

        public async Task<MessageSetting?> GetByID(int messageSettingId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.MessageSettingID == messageSettingId);
        }

        // Matched the way MessageService matches — case, spaces and punctuation ignored — so a row
        // the admin named "Sender Registration" is found by code asking for "SenderRegistration".
        public async Task<MessageSetting?> GetByType(string type)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => MessageEventKey.Matches(x.Type, type));
        }

        public async Task<int> Insert(MessageSetting entity)
        {
            await _context.MessageSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllMessageSettings);
            return result;
        }

        public async Task<int> Update(MessageSetting entity)
        {
            _context.MessageSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllMessageSettings);
            return result;
        }

        public async Task<int> Delete(int messageSettingId)
        {
            var entity = await _context.MessageSettings.FindAsync(messageSettingId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllMessageSettings);
            return result;
        }
    }
}
