using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserwiseServiceChargeMasterRepository
    {
        Task<List<UserwiseServiceChargeMaster>> GetAll();
        Task<UserwiseServiceChargeMaster?> GetByID(int id);
        Task<List<UserwiseServiceChargeMaster>> GetByUserID(int userid);
        Task<int> Insert(UserwiseServiceChargeMaster entity);
        Task<int> Update(UserwiseServiceChargeMaster entity);
        Task<int> Delete(int id);
    }
}
