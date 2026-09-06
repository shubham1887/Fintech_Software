using FintechApp.Requests.CreditCardSetting;
using FintechApp.Responses.CreditCardSetting;
namespace FintechApp.Interfaces.Services
{
    public interface ICreditCardSettingService
    {
        Task<List<CreditCardSettingResponse>> GetAll();
        Task<CreditCardSettingResponse?> GetByID(int id);
        Task<string> Create(CreateCreditCardSettingRequest request);
        Task<string> Update(UpdateCreditCardSettingRequest request);
        Task<string> Delete(int id);
    }
}
