using FintechApp.Requests.LeadGenerationSetting;
using FintechApp.Responses.LeadGenerationSetting;
namespace FintechApp.Interfaces.Services
{
    public interface ILeadGenerationSettingService
    {
        Task<List<LeadGenerationSettingResponse>> GetAll();
        Task<LeadGenerationSettingResponse?> GetByID(int id);
        Task<string> Create(CreateLeadGenerationSettingRequest request);
        Task<string> Update(UpdateLeadGenerationSettingRequest request);
        Task<string> Delete(int id);
    }
}
