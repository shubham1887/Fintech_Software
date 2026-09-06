using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IRangeWiseSequenceRepository
    {
        Task<List<RangeWiseSequence>> GetAll();
        Task<List<RangeWiseSequence>> GetByOperatorID(int operatorId);
        Task<RangeWiseSequence?> GetByID(int id);
        Task<int> Insert(RangeWiseSequence entity);
        Task<int> Update(RangeWiseSequence entity);
        Task<int> Delete(int id);
    }
}
