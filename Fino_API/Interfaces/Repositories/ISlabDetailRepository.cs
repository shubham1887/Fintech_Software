using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ISlabDetailRepository
    {
        Task<List<SlabDetail>> GetAll();
        Task<List<SlabDetail>> GetBySlabMasterID(int slabMasterId);
        Task<SlabDetail?> GetByID(int slabDetailId);
        Task<int> Insert(SlabDetail entity);
        Task<int> Update(SlabDetail entity);
        Task<int> Delete(int slabDetailId);
    }
}
