using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.IPMaster;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class IPMasterRepository : IIPMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<IPMaster>> _listCache;

        public IPMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<IPMaster>>();
        }

        public async Task<List<IPMaster>> GetAllIPMasters()
        {
            return _listCache.GetOrCreate(CacheKeys.AllIPMasters, () =>
            {
                return _context.IPMasters
                    .Include(i => i.UserMaster)
                    .Where(i => !i.IsDeleted)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<IPMaster>();
        }

        public async Task<IPMaster?> GetIPMasterById(int ipMasterID)
        {
            var all = await GetAllIPMasters();
            return all.FirstOrDefault(i => i.IpMasterID == ipMasterID);
        }

        public async Task<IPMaster?> GetIPMasterByIP(string ip)
        {
            var all = await GetAllIPMasters();
            return all.FirstOrDefault(i => i.IP.Equals(ip, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<IPMaster>> GetIPMasters(GetIPMasterRequest request)
        {
            var all = await GetAllIPMasters();
            var query = all.AsQueryable();

            if (request.IpMasterID > 0)
                query = query.Where(i => i.IpMasterID == request.IpMasterID);

            if (request.UserID > 0)
                query = query.Where(i => i.UserID == request.UserID);

            if (!string.IsNullOrEmpty(request.IP))
                query = query.Where(i => i.IP.Contains(request.IP, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(i => i.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            var skip = (request.PageNumber - 1) * request.PageSize;
            return query.Skip(skip).Take(request.PageSize).ToList();
        }

        public async Task<int> Insert(IPMaster ipMaster)
        {
            await _context.IPMasters.AddAsync(ipMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearIPMasterCache();

            return result;
        }

        public async Task<int> Update(IPMaster ipMaster)
        {
            _context.IPMasters.Update(ipMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearIPMasterCache();

            return result;
        }

        public async Task<int> Delete(IPMaster ipMaster)
        {
            _context.IPMasters.Update(ipMaster);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearIPMasterCache();

            return result;
        }

        private void ClearIPMasterCache()
        {
            _listCache.Remove(CacheKeys.AllIPMasters);
        }
    }
}