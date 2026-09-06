using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUpiBeneficiaryRepository
    {
        Task<List<UpiBeneficiary>> GetAll();
        Task<UpiBeneficiary?> GetByID(int id);
        Task<List<UpiBeneficiary>> GetByUserID(int userid);
        Task<int> Insert(UpiBeneficiary entity);
        Task<int> Update(UpiBeneficiary entity);
        Task<int> Delete(int id);
    }
}
