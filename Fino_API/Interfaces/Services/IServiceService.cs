using FintechApp.Requests.Service;
using FintechApp.Responses.Service;

namespace FintechApp.Interfaces.Services
{
    public interface IServiceService
    {
        Task<ServiceResponse?> GetServiceById(int serviceId);
        Task<List<ServiceResponse>> GetServices(GetServiceRequest request);
        Task<string> CreateService(CreateServiceRequest request);
        Task<string> UpdateService(UpdateServiceRequest request);
        Task<string> DeleteService(int serviceId);
        Task<List<ServiceDropdownResponse>> GetServiceDropdown();
        Task<List<ServiceCatalogResponse>> GetServiceCatalog(bool includeHidden, int userID = 0);
    }
}