using FintechApp.Requests.LeadGenerationMaster;
using FintechApp.Responses.LeadGenerationMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ILeadGenerationMasterService
    {
        Task<List<LeadGenerationMasterResponse>> GetAll();
        Task<LeadGenerationMasterResponse?> GetByID(int id);

        Task<string> Create(CreateLeadGenerationMasterRequest request);
        Task<string> Update(UpdateLeadGenerationMasterRequest request);
        Task<string> Delete(int id);
    }
}
