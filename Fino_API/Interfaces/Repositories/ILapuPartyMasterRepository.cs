using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILapuPartyMasterRepository
    {
        Task<List<LapuPartyMaster>> GetAll();
        Task<LapuPartyMaster?> GetByID(int id);
        Task<int> Insert(LapuPartyMaster entity);
        Task<int> Update(LapuPartyMaster entity);
        Task<int> Delete(int id);
    }
}
