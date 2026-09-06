using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IVirtualAccountMasterRepository
    {
        Task<List<VirtualAccountMaster>> GetAll();
        Task<VirtualAccountMaster?> GetByID(int id);

        Task<int> Insert(VirtualAccountMaster entity);
        Task<int> Update(VirtualAccountMaster entity);
        Task<int> Delete(int id);
    }
}
