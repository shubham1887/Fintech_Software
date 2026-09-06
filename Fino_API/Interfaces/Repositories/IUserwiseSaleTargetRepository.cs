using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserwiseSaleTargetRepository
    {
        Task<List<UserwiseSaleTarget>> GetAll();
        Task<UserwiseSaleTarget?> GetByID(int id);
        Task<List<UserwiseSaleTarget>> GetByUserID(int userid);
        Task<int> Insert(UserwiseSaleTarget entity);
        Task<int> Update(UserwiseSaleTarget entity);
        Task<int> Delete(int id);
    }
}
