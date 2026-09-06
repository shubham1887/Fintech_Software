using FintechApp.Requests.LapuMaster;
using FintechApp.Responses.LapuMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ILapuMasterService
    {
        Task<List<LapuMasterResponse>> GetAll();
        Task<LapuMasterResponse?> GetByID(int id);
        Task<string> Create(CreateLapuMasterRequest request);
        Task<string> Update(UpdateLapuMasterRequest request);
        Task<string> Delete(int id);
    }
}
