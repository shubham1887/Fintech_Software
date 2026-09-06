using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICircleWiseOperatorLimitRepository
    {
        Task<List<CircleWiseOperatorLimit>> GetAll();
        Task<CircleWiseOperatorLimit?> GetByID(int id);
        Task<List<CircleWiseOperatorLimit>> GetByUserID(int userID);
        Task<int> Insert(CircleWiseOperatorLimit entity);
        Task<int> Update(CircleWiseOperatorLimit entity);
        Task<int> Delete(int id);
    }
}
