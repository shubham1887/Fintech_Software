using FintechApp.Requests.Beneficiary;
using FintechApp.Responses.Beneficiary;
namespace FintechApp.Interfaces.Services
{
    public interface IBeneficiaryService
    {
        Task<List<BeneficiaryResponse>> GetAll();
        Task<BeneficiaryResponse?> GetByID(int id);
        Task<List<BeneficiaryResponse>> GetByUserID(int userid);
        Task<string> Create(CreateBeneficiaryRequest request);
        Task<string> Update(UpdateBeneficiaryRequest request);
        Task<string> Delete(int id);
    }
}
