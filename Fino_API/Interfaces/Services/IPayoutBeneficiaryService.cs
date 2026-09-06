using FintechApp.Requests.PayoutBeneficiary;
using FintechApp.Responses.PayoutBeneficiary;

namespace FintechApp.Interfaces.Services
{
    public interface IPayoutBeneficiaryService
    {
        Task<List<PayoutBeneficiaryResponse>> GetAll();
        Task<PayoutBeneficiaryResponse?> GetByID(int id);
        Task<List<PayoutBeneficiaryResponse>> GetByUserID(int userID);
        Task<string> Create(CreatePayoutBeneficiaryRequest request);
        Task<string> Update(UpdatePayoutBeneficiaryRequest request);
        Task<string> Delete(int id);
    }
}
