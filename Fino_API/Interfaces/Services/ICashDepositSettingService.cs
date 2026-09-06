using FintechApp.Requests.CashDepositSetting;
using FintechApp.Responses.CashDepositSetting;
namespace FintechApp.Interfaces.Services
{
    public interface ICashDepositSettingService
    {
        Task<List<CashDepositSettingResponse>> GetAll();
        Task<CashDepositSettingResponse?> GetByID(int id);
        Task<string> Create(CreateCashDepositSettingRequest request);
        Task<string> Update(UpdateCashDepositSettingRequest request);
        Task<string> Delete(int id);
    }
}
