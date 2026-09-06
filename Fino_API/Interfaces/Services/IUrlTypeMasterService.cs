using FintechApp.Requests.UrlTypeMaster;
using FintechApp.Responses.UrlTypeMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IUrlTypeMasterService
    {
        Task<List<UrlTypeMasterResponse>> GetAll();
        Task<UrlTypeMasterResponse?> GetByID(int id);

        Task<string> Create(CreateUrlTypeMasterRequest request);
        Task<string> Update(UpdateUrlTypeMasterRequest request);
        Task<string> Delete(int id);
    }
}
