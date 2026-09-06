using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISubCategoryRepository
    {
        Task<List<SubCategory>> GetAll();
        Task<SubCategory?> GetByID(int id);

        Task<int> Insert(SubCategory entity);
        Task<int> Update(SubCategory entity);
        Task<int> Delete(int id);
    }
}
