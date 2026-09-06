using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IMenuActionMappingRepository
    {
        Task<List<MenuActionMapping>> GetAll();
        Task<MenuActionMapping?> GetByID(int id);

        Task<int> Insert(MenuActionMapping entity);
        Task<int> Update(MenuActionMapping entity);
        Task<int> Delete(int id);
    }
}
