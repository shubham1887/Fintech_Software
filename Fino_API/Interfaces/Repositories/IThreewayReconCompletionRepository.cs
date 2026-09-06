using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IThreewayReconCompletionRepository
    {
        Task<List<ThreewayReconCompletion>> GetAll();
        Task<ThreewayReconCompletion?> GetByID(int id);

        Task<int> Insert(ThreewayReconCompletion entity);
        Task<int> Update(ThreewayReconCompletion entity);
        Task<int> Delete(int id);
    }
}
