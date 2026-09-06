using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUpiSenderRepository
    {
        Task<List<UpiSender>> GetAll();
        Task<UpiSender?> GetByID(int id);
        Task<List<UpiSender>> GetByUserID(int userid);
        Task<int> Insert(UpiSender entity);
        Task<int> Update(UpiSender entity);
        Task<int> Delete(int id);
    }
}
