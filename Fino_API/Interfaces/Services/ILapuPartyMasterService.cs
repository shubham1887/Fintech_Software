using FintechApp.Requests.LapuPartyMaster;
using FintechApp.Responses.LapuPartyMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ILapuPartyMasterService
    {
        Task<List<LapuPartyMasterResponse>> GetAll();
        Task<LapuPartyMasterResponse?> GetByID(int id);
        Task<string> Create(CreateLapuPartyMasterRequest request);
        Task<string> Update(UpdateLapuPartyMasterRequest request);
        Task<string> Delete(int id);
    }
}
