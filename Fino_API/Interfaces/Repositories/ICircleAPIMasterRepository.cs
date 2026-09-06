using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICircleAPIMasterRepository
    {
        Task<List<CircleAPIMaster>> GetAll();
        Task<CircleAPIMaster?> GetByID(int id);
        Task<int> Insert(CircleAPIMaster entity);
        Task<int> Update(CircleAPIMaster entity);
        Task<int> Delete(int id);
    }
}
