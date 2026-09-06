using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IComplaintMasterRepository
    {
        Task<List<ComplaintMaster>> GetAll();
        Task<List<ComplaintMaster>> GetByUserID(int userID);
        Task<ComplaintMaster?> GetByID(int id);
        Task<int> Insert(ComplaintMaster entity);
        Task<int> Update(ComplaintMaster entity);
        Task<int> Delete(int id);
    }
}
