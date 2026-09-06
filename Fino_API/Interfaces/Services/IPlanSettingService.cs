using FintechApp.Requests.PlanSetting;
using FintechApp.Responses.PlanSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IPlanSettingService
    {
        Task<List<PlanSettingResponse>> GetAll();
        Task<PlanSettingResponse?> GetByID(int id);
        Task<string> Create(CreatePlanSettingRequest request);
        Task<string> Update(UpdatePlanSettingRequest request);
        Task<string> Delete(int id);
    }
}
