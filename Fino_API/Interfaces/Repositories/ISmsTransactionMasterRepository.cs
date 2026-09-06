using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISmsTransactionMasterRepository
    {
        Task<List<SmsTransactionMaster>> GetAll();
        Task<SmsTransactionMaster?> GetByID(int id);

        Task<int> Insert(SmsTransactionMaster entity);
        Task<int> Update(SmsTransactionMaster entity);
        Task<int> Delete(int id);
    }
}
