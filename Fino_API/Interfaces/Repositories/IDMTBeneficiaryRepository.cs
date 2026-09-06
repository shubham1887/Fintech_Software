using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IDMTBeneficiaryRepository
    {
        Task<List<DMTBeneficiary>> GetAll();
        Task<DMTBeneficiary?> GetByID(int id);
        Task<List<DMTBeneficiary>> GetBySenderID(int senderId);
        Task<DMTBeneficiary?> GetByAccountAndSender(string accountNo, int senderId);
        Task<int> Insert(DMTBeneficiary entity);
        Task<int> Update(DMTBeneficiary entity);
        Task<int> Delete(int id);
    }
}
