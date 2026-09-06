using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IMediaMasterRepository
    {
        Task<List<MediaMaster>> GetAll();
        Task<MediaMaster?> GetByID(int id);
        Task<List<MediaMaster>> GetByUserID(int userID);
        Task<int> Insert(MediaMaster entity);
        Task<int> Update(MediaMaster entity);
        Task<int> Delete(int id);
    }
}
