using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOperatorUserwiseRepository
    {
        Task<List<OperatorUserwise>> GetAll();
        Task<List<OperatorUserwise>> GetByUserID(int userId);
        Task<OperatorUserwise?> GetByID(int id);
        Task<int> Insert(OperatorUserwise entity);
        Task<int> Update(OperatorUserwise entity);
        Task<int> Delete(int id);
    }
}
