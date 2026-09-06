using FintechApp.Requests.ZoneMaster;
using FintechApp.Responses.ZoneMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IZoneMasterService
    {
        Task<List<ZoneMasterResponse>> GetAll();
        Task<ZoneMasterResponse?> GetByID(int id);
        Task<string> Create(CreateZoneMasterRequest request);
        Task<string> Update(UpdateZoneMasterRequest request);
        Task<string> Delete(int id);
    }
}
