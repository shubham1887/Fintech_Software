using FintechApp.Requests.AndroidMenuMaster;
using FintechApp.Responses.AndroidMenuMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IAndroidMenuMasterService
    {
        Task<List<AndroidMenuMasterResponse>> GetAll();
        Task<AndroidMenuMasterResponse?> GetByID(int id);

        Task<string> Create(CreateAndroidMenuMasterRequest request);
        Task<string> Update(UpdateAndroidMenuMasterRequest request);
        Task<string> Delete(int id);
    }
}
