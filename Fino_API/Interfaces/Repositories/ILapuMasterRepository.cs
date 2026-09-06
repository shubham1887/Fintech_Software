using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILapuMasterRepository
    {
        Task<List<LapuMaster>> GetAll();
        Task<LapuMaster?> GetByID(int id);
        Task<int> Insert(LapuMaster entity);
        Task<int> Update(LapuMaster entity);
        Task<int> Delete(int id);
    }
}
