using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IDenominationMasterRepository
    {
        Task<List<DenominationMaster>> GetAll();
        Task<DenominationMaster?> GetByID(int denominationMasterId);
        Task<int> Insert(DenominationMaster entity);
        Task<int> Update(DenominationMaster entity);
        Task<int> Delete(int denominationMasterId);
    }
}
