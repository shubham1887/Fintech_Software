using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISignUpRepository
    {
        Task<List<SignUp>> GetAll();
        Task<SignUp?> GetByID(int id);

        Task<int> Insert(SignUp entity);
        Task<int> Update(SignUp entity);
        Task<int> Delete(int id);
    }
}
