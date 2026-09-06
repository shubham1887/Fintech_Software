using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICircleWiseUserWiseOperatorOffRepository
    {
        Task<List<CircleWiseUserWiseOperatorOff>> GetAll();
        Task<CircleWiseUserWiseOperatorOff?> GetByID(int id);
        Task<List<CircleWiseUserWiseOperatorOff>> GetByUserID(int userID);
        Task<int> Insert(CircleWiseUserWiseOperatorOff entity);
        Task<int> Update(CircleWiseUserWiseOperatorOff entity);
        Task<int> Delete(int id);
    }
}
