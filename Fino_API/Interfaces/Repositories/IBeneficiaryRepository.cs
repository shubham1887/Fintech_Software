using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBeneficiaryRepository
    {
        Task<List<Beneficiary>> GetAll();
        Task<Beneficiary?> GetByID(int id);
        Task<List<Beneficiary>> GetByUserID(int userid);
        Task<int> Insert(Beneficiary entity);
        Task<int> Update(Beneficiary entity);
        Task<int> Delete(int id);
    }
}
