using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOperatorSequenceRepository
    {
        Task<List<OperatorSequence>> GetAll();
        Task<OperatorSequence?> GetByID(int id);
        Task<List<OperatorSequence>> GetByOperatorID(int operatorID);
        Task<OperatorSequence?> GetByOperatorAndProvider(int operatorID, int providerID);
        Task<int> Insert(OperatorSequence entity);
        Task<int> Update(OperatorSequence entity);
        Task<int> Delete(int id);
        Task<int> DeleteByOperatorID(int operatorID);
        Task<int> DeleteByOperatorAndProvider(int operatorID, int providerID);
    }
}
