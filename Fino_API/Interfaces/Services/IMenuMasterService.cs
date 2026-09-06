using FintechApp.Requests.MenuMaster;
using FintechApp.Responses.MenuMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IMenuMasterService
    {
        Task<List<MenuMasterResponse>> GetAll();
        Task<MenuMasterResponse?> GetByID(int id);
        Task<string> Create(CreateMenuMasterRequest request);
        Task<string> Update(UpdateMenuMasterRequest request);
        Task<string> Delete(int id);
    }
}
