using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILocationLookUpRepository
    {
        Task<List<LocationLookUp>> GetAll();
        Task<LocationLookUp?> GetByID(int id);
        Task<int> Insert(LocationLookUp entity);
        Task<int> Update(LocationLookUp entity);
        Task<int> Delete(int id);
    }
}
