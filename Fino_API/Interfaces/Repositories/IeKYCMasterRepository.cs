using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IeKYCMasterRepository
    {
        Task<List<eKYCMaster>> GetAll();
        Task<eKYCMaster?> GetByID(int id);
        Task<List<eKYCMaster>> GetByUserID(int userid);
        Task<int> Insert(eKYCMaster entity);
        Task<int> Update(eKYCMaster entity);
        Task<int> Delete(int id);
    }
}
