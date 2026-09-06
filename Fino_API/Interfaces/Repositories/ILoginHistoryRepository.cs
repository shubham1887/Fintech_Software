using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ILoginHistoryRepository
    {
        Task<List<LoginHistory>> GetByUserID(int userId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);
        Task<int> Insert(LoginHistory entity);
    }
}
