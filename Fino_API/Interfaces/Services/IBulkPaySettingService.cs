using FintechApp.Requests.BulkPaySetting;
using FintechApp.Responses.BulkPaySetting;
namespace FintechApp.Interfaces.Services
{
    public interface IBulkPaySettingService
    {
        Task<List<BulkPaySettingResponse>> GetAll();
        Task<BulkPaySettingResponse?> GetByID(int id);

        Task<string> Create(CreateBulkPaySettingRequest request);
        Task<string> Update(UpdateBulkPaySettingRequest request);
        Task<string> Delete(int id);
    }
}
