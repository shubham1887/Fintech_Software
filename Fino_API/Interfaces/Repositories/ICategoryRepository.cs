using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAll();
        Task<Category?> GetByID(int id);

        Task<int> Insert(Category entity);
        Task<int> Update(Category entity);
        Task<int> Delete(int id);
    }
}
