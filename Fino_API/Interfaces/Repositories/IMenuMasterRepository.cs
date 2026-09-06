using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IMenuMasterRepository
    {
        Task<List<MenuMaster>> GetAll();
        Task<MenuMaster?> GetByID(int id);
        Task<int> Insert(MenuMaster entity);
        Task<int> Update(MenuMaster entity);
        Task<int> Delete(int id);
    }
}
