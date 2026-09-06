using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ICircleWiseCommissionRepository
    {
        Task<List<CircleWiseCommission>> GetAll();
        Task<CircleWiseCommission?> GetByID(int id);
        Task<List<CircleWiseCommission>> GetBySlabMasterID(int slabMasterID);
        Task<int> Insert(CircleWiseCommission entity);
        Task<int> Update(CircleWiseCommission entity);
        Task<int> Delete(int id);
    }
}
