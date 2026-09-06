using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUnitMasterRepository
    {
        Task<List<UnitMaster>> GetAll();
        Task<UnitMaster?> GetByID(int id);

        Task<int> Insert(UnitMaster entity);
        Task<int> Update(UnitMaster entity);
        Task<int> Delete(int id);
    }
}
