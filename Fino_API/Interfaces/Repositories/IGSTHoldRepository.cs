using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IGSTHoldRepository
    {
        Task<List<GSTHold>> GetAll();
        Task<GSTHold?> GetByID(int id);
        Task<List<GSTHold>> GetByUserID(int userID);
        Task<int> Insert(GSTHold entity);
        Task<int> Update(GSTHold entity);
        Task<int> Delete(int id);
    }
}
