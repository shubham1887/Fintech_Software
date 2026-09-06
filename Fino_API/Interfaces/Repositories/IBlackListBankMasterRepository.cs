using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBlackListBankMasterRepository
    {
        Task<List<BlackListBankMaster>> GetAll();
        Task<BlackListBankMaster?> GetByID(int id);

        Task<int> Insert(BlackListBankMaster entity);
        Task<int> Update(BlackListBankMaster entity);
        Task<int> Delete(int id);
    }
}
