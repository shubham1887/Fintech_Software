using FintechApp.Requests.ROfferCommission;
using FintechApp.Responses.ROfferCommission;
namespace FintechApp.Interfaces.Services
{
    public interface IROfferCommissionService
    {
        Task<List<ROfferCommissionResponse>> GetAll();
        Task<ROfferCommissionResponse?> GetByID(int id);
        Task<List<ROfferCommissionResponse>> GetByOperatorID(int operatorID);
        Task<string> Create(CreateROfferCommissionRequest request);
        Task<string> Update(UpdateROfferCommissionRequest request);
        Task<string> Delete(int id);
    }
}
