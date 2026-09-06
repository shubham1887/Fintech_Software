using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILapuTransactionDetailRepository
    {
        Task<List<LapuTransactionDetail>> GetAll();
        Task<LapuTransactionDetail?> GetByID(int id);
        Task<List<LapuTransactionDetail>> GetByUserID(int userid);
        Task<int> Insert(LapuTransactionDetail entity);
        Task<int> Update(LapuTransactionDetail entity);
        Task<int> Delete(int id);
    }
}
