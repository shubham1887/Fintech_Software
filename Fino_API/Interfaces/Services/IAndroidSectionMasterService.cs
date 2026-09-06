using FintechApp.Requests.AndroidSectionMaster;
using FintechApp.Responses.AndroidSectionMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IAndroidSectionMasterService
    {
        Task<List<AndroidSectionMasterResponse>> GetAll();
        Task<AndroidSectionMasterResponse?> GetByID(int id);

        Task<string> Create(CreateAndroidSectionMasterRequest request);
        Task<string> Update(UpdateAndroidSectionMasterRequest request);
        Task<string> Delete(int id);
    }
}
