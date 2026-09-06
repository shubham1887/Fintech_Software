using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserPaymentCollectMappingRepository
    {
        Task<List<UserPaymentCollectMapping>> GetAll();
        Task<UserPaymentCollectMapping?> GetByID(int id);
        Task<List<UserPaymentCollectMapping>> GetByUserID(int userid);
        Task<int> Insert(UserPaymentCollectMapping entity);
        Task<int> Update(UserPaymentCollectMapping entity);
        Task<int> Delete(int id);
    }
}
