using FintechApp.Requests.MenuActionMapping;
using FintechApp.Responses.MenuActionMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IMenuActionMappingService
    {
        Task<List<MenuActionMappingResponse>> GetAll();
        Task<MenuActionMappingResponse?> GetByID(int id);

        Task<string> Create(CreateMenuActionMappingRequest request);
        Task<string> Update(UpdateMenuActionMappingRequest request);
        Task<string> Delete(int id);
    }
}
