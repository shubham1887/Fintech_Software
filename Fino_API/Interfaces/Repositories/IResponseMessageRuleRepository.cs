using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IResponseMessageRuleRepository
    {
        Task<List<ResponseMessageRule>> GetAll();
        Task<ResponseMessageRule?> GetByID(int id);
        Task<int> Insert(ResponseMessageRule entity);
        Task<int> Update(ResponseMessageRule entity);
        Task<int> Delete(int id);
    }
}
