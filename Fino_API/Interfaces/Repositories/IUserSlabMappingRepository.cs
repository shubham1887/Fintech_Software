using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserSlabMappingRepository
    {
        Task<List<UserSlabMapping>> GetAll();
        Task<List<UserSlabMapping>> GetByUserID(int userId);
        Task<UserSlabMapping?> GetByID(int userSlabMappingId);
        Task<int> Insert(UserSlabMapping entity);
        Task<int> Update(UserSlabMapping entity);
        Task<int> Delete(int userSlabMappingId);
    }
}
