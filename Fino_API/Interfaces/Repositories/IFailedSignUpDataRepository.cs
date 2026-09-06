using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IFailedSignUpDataRepository
    {
        Task<List<FailedSignUpData>> GetAll();
        Task<FailedSignUpData?> GetByID(int id);

        Task<int> Insert(FailedSignUpData entity);
        Task<int> Update(FailedSignUpData entity);
        Task<int> Delete(int id);
    }
}
