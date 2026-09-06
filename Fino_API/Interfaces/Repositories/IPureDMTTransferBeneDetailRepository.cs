using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPureDMTTransferBeneDetailRepository
    {
        Task<List<PureDMTTransferBeneDetail>> GetAll();
        Task<PureDMTTransferBeneDetail?> GetByID(int id);

        Task<int> Insert(PureDMTTransferBeneDetail entity);
        Task<int> Update(PureDMTTransferBeneDetail entity);
        Task<int> Delete(int id);
    }
}
