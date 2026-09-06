using FintechApp.Requests.PayoutSetting;
using FintechApp.Responses.PayoutSetting;

namespace FintechApp.IServices
{
    public interface IPayoutSettingService
    {
        Task<List<PayoutSettingResponse>> GetAll();
        Task<PayoutSettingResponse?> GetByID(int id);
        Task<string> Create(CreatePayoutSettingRequest request);
        Task<string> Update(UpdatePayoutSettingRequest request);
        Task<string> Delete(int id);
    }
}
