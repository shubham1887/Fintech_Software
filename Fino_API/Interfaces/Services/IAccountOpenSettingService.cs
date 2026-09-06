using FintechApp.Requests.AccountOpenSetting;
using FintechApp.Responses.AccountOpenSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IAccountOpenSettingService
    {
        Task<List<AccountOpenSettingResponse>> GetAll();
        Task<AccountOpenSettingResponse?> GetByID(int id);
        Task<string> Create(CreateAccountOpenSettingRequest request);
        Task<string> Update(UpdateAccountOpenSettingRequest request);
        Task<string> Delete(int id);
    }
}
