using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ISlabMasterRepository
    {
        Task<List<SlabMaster>> GetAll();
        Task<SlabMaster?> GetByID(int slabMasterId);
        Task<SlabMaster?> GetByName(string slabName);
        Task<int> Insert(SlabMaster entity);
        Task<int> Update(SlabMaster entity);
        Task<int> Delete(int slabMasterId);
    }
}
