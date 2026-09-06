using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.Service;
using FintechApp.Responses.Service;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ServiceMaster>> _listCache;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ServiceMaster>>();
        }

        public async Task<List<ServiceMaster>> GetAllServices()
        {
            return _listCache.GetOrCreate(CacheKeys.AllServices, () =>
            {
                return _context.ServiceMasters
                    .Where(s => !s.IsDeleted)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<ServiceMaster>();
        }

        public async Task<ServiceMaster?> GetServiceById(int serviceId)
        {
            var all = await GetAllServices();
            return all.FirstOrDefault(s => s.ServiceID == serviceId);
        }

        public async Task<List<ServiceMaster>> GetServicesByCategory(string category)
        {
            var all = await GetAllServices();
            return all
                .Where(s => s.ServiceCategory.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<List<ServiceMaster>> GetServices(GetServiceRequest request)
        {
            var all = await GetAllServices();
            var query = all.AsQueryable();

            if (request.ServiceID > 0)
                query = query.Where(s => s.ServiceID == request.ServiceID);

            if (!string.IsNullOrEmpty(request.Name))
                query = query.Where(s => s.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.ServiceCategory))
                query = query.Where(s => s.ServiceCategory.Equals(request.ServiceCategory, StringComparison.OrdinalIgnoreCase));

            if (request.IsActive.HasValue)
                query = query.Where(s => s.IsActive == request.IsActive.Value);

            return query.ToList();
        }

        public async Task<int> Insert(ServiceMaster service)
        {
            await _context.ServiceMasters.AddAsync(service);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearServiceCache();

            return result;
        }

        public async Task<int> Update(ServiceMaster service)
        {
            _context.ServiceMasters.Update(service);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearServiceCache();

            return result;
        }

        public async Task<int> Delete(ServiceMaster service)
        {
            _context.ServiceMasters.Update(service);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearServiceCache();

            return result;
        }

        public async Task<List<ServiceMaster>> GetActiveServices()
        {
            var all = await GetAllServices();
            return all
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToList();
        }

        public async Task<List<ServiceDropdownResponse>> GetDropDownServices()
        {
            var allServices = await GetActiveServices();
            return allServices
                .Select(s => new ServiceDropdownResponse
                {
                    ServiceID = s.ServiceID,
                    Name = s.Name
                }).ToList();
        }

        private void ClearServiceCache()
        {
            _listCache.Remove(CacheKeys.AllServices);
        }
    }
}