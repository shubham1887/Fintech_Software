using FintechApp.Requests.DenominationMaster;
using FintechApp.Responses.DenominationMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IDenominationMasterService
    {
        Task<List<DenominationMasterResponse>> GetAll();
        Task<DenominationMasterResponse?> GetByID(int denominationMasterId);
        Task<string> Create(CreateDenominationMasterRequest request);
        Task<string> Update(UpdateDenominationMasterRequest request);
        Task<string> Delete(int denominationMasterId);
    }
}
