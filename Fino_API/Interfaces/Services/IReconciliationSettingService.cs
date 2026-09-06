using FintechApp.Requests.ReconciliationSetting;
using FintechApp.Responses.ReconciliationSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IReconciliationSettingService
    {
        Task<List<ReconciliationSettingResponse>> GetAll();
        Task<List<ReconciliationSettingResponse>> GetByProviderID(int providerId);
        Task<ReconciliationSettingResponse?> GetByID(int id);
        Task<string> Create(CreateReconciliationSettingRequest request);
        Task<string> Update(UpdateReconciliationSettingRequest request);
        Task<string> Delete(int id);
    }
}
