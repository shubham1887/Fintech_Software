using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITransactionAPIUserRequestMasterRepository
    {
        Task<List<TransactionAPIUserRequestMaster>> GetAll();
        Task<TransactionAPIUserRequestMaster?> GetByID(int id);
        Task<List<TransactionAPIUserRequestMaster>> GetByUserID(int userid);
        Task<int> Insert(TransactionAPIUserRequestMaster entity);
        Task<int> Update(TransactionAPIUserRequestMaster entity);
        Task<int> Delete(int id);
    }
}
