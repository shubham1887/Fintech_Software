using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILastChangeUserDetailsRepository
    {
        Task<List<LastChangeUserDetails>> GetAll();
        Task<LastChangeUserDetails?> GetByID(int id);

        Task<int> Insert(LastChangeUserDetails entity);
        Task<int> Update(LastChangeUserDetails entity);
        Task<int> Delete(int id);
    }
}
