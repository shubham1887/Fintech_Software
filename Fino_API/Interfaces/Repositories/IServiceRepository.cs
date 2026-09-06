using FintechApp.Entities;
using FintechApp.Requests.Service;
using FintechApp.Responses.Service;

namespace FintechApp.Interfaces.Repositories
{
    public interface IServiceRepository
    {
        Task<List<ServiceMaster>> GetAllServices();
        Task<ServiceMaster?> GetServiceById(int serviceId);
        Task<List<ServiceMaster>> GetServicesByCategory(string category);
        Task<List<ServiceMaster>> GetServices(GetServiceRequest request);
        Task<int> Insert(ServiceMaster service);
        Task<int> Update(ServiceMaster service);
        Task<int> Delete(ServiceMaster service);
        Task<List<ServiceMaster>> GetActiveServices();
        Task<List<ServiceDropdownResponse>> GetDropDownServices();
    }
}