using FintechApp.Requests.DMTBeneficiary;
using FintechApp.Responses.DMTBeneficiary;

namespace FintechApp.Interfaces.Services
{
    public interface IDMTBeneficiaryService
    {
        Task<List<DMTBeneficiaryResponse>> GetAll(GetDMTBeneficiaryRequest request);
        Task<string> Create(CreateDMTBeneficiaryRequest request);
        Task<string> Update(UpdateDMTBeneficiaryRequest request);
        Task<string> Delete(DeleteDMTBeneficiaryRequest request);
    }
}
