using FintechApp.Requests.SmsMaster;
using FintechApp.Responses.SmsMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ISmsMasterService
    {
        Task<List<SmsMasterResponse>> GetAll();
        Task<SmsMasterResponse?> GetByID(int smsMasterId);
        Task<string> Create(CreateSmsMasterRequest request);
        Task<string> Update(UpdateSmsMasterRequest request);
        Task<string> Delete(int smsMasterId);
    }
}
