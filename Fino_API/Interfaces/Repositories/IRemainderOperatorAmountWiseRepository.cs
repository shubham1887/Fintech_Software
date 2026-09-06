using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IRemainderOperatorAmountWiseRepository
    {
        Task<List<RemainderOperatorAmountWise>> GetAll();
        Task<RemainderOperatorAmountWise?> GetByID(int id);

        Task<int> Insert(RemainderOperatorAmountWise entity);
        Task<int> Update(RemainderOperatorAmountWise entity);
        Task<int> Delete(int id);
    }
}
