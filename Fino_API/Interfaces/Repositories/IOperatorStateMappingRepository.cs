using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOperatorStateMappingRepository
    {
        Task<List<OperatorStateMapping>> GetAll();
        Task<OperatorStateMapping?> GetByID(int id);
        Task<List<OperatorStateMapping>> GetByOperatorID(int operatorID);
        Task<int> Insert(OperatorStateMapping entity);
        Task<int> Update(OperatorStateMapping entity);
        Task<int> Delete(int id);
    }
}
