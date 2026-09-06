using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IVirtualBalanceRepository
    {
        Task<List<VirtualBalance>> GetAll();
        Task<VirtualBalance?> GetByID(int id);

        Task<int> Insert(VirtualBalance entity);
        Task<int> Update(VirtualBalance entity);
        Task<int> Delete(int id);
    }
}
