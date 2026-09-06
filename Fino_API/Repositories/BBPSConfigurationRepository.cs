using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.BBPSConfiguration;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class BBPSConfigurationRepository : IBBPSConfigurationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<BBPSConfiguration>> _listCache;

        public BBPSConfigurationRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<BBPSConfiguration>>();
        }

        public async Task<List<BBPSConfiguration>> GetAllBBPSConfigurations()
        {
            return _listCache.GetOrCreate(CacheKeys.AllBBPSConfigurations, () =>
            {
                return _context.BBPSConfigurations
                    .Include(b => b.OperatorMaster)
                    .Where(b => !b.IsDeleted)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<BBPSConfiguration>();
        }

        public async Task<BBPSConfiguration?> GetBBPSConfigurationById(int bbpsConfigurationID)
        {
            var all = await GetAllBBPSConfigurations();
            return all.FirstOrDefault(b => b.BBPSConfigurationID == bbpsConfigurationID);
        }

        public async Task<List<BBPSConfiguration>> GetBBPSConfigurations(GetBBPSConfigurationRequest request)
        {
            var all = await GetAllBBPSConfigurations();
            var query = all.AsQueryable();

            if (request.BBPSConfigurationID > 0)
                query = query.Where(b => b.BBPSConfigurationID == request.BBPSConfigurationID);

            if (request.OperatorID > 0)
                query = query.Where(b => b.OperatorID == request.OperatorID);

            if (!string.IsNullOrEmpty(request.Label))
                query = query.Where(b => b.Label.Contains(request.Label, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(b => b.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase));

            var skip = (request.PageNumber - 1) * request.PageSize;
            return query.Skip(skip).Take(request.PageSize).ToList();
        }

        public async Task<bool> CreateBBPSConfiguration(BBPSConfiguration bbpsConfiguration)
        {
            bbpsConfiguration.DateTime = DateTime.UtcNow;
            bbpsConfiguration.UpdatedateTime = DateTime.UtcNow;
            bbpsConfiguration.IsDeleted = false;

            await _context.BBPSConfigurations.AddAsync(bbpsConfiguration);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearBBPSConfigurationCache();


            return result > 0;
        }
            
        public async Task<bool> UpdateBBPSConfiguration(BBPSConfiguration bbpsConfiguration)
        {
            bbpsConfiguration.UpdatedateTime = DateTime.UtcNow;

            _context.BBPSConfigurations.Update(bbpsConfiguration);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearBBPSConfigurationCache();

            return result > 0;
        }

        public async Task<bool> DeleteBBPSConfiguration(BBPSConfiguration bbpsConfiguration)
        {
            bbpsConfiguration.UpdatedateTime = DateTime.UtcNow;

            _context.BBPSConfigurations.Update(bbpsConfiguration);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearBBPSConfigurationCache();

            return result >0;
        }

        public void ClearBBPSConfigurationCache()
        {
            _listCache.Remove(CacheKeys.AllBBPSConfigurations);
        }
    }
}