using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOperatorSequenceUserWiseRepository
    {
        Task<List<OperatorSequenceUserWise>> GetAll();
        Task<List<OperatorSequenceUserWise>> GetByUserID(int userId);
        Task<OperatorSequenceUserWise?> GetByID(int id);
        Task<List<OperatorSequenceUserWise>> GetByUserAndOperator(int userId, int operatorID);
        Task<int> Insert(OperatorSequenceUserWise entity);
        Task<int> Update(OperatorSequenceUserWise entity);
        Task<int> Delete(int id);
        Task<int> DeleteByUserAndOperator(int userId, int operatorID);
        Task<int> DeleteAllRows();
    }
}
