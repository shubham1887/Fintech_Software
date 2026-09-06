using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISupportUserAccessMasterRepository
    {
        Task<List<SupportUserAccessMaster>> GetAll();
        Task<SupportUserAccessMaster?> GetByID(int id);
        Task<List<SupportUserAccessMaster>> GetByUserID(int userid);
        Task<int> Insert(SupportUserAccessMaster entity);
        Task<int> Update(SupportUserAccessMaster entity);
        Task<int> Delete(int id);
    }
}
