using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICallbackMasterRepository
    {
        Task<List<CallbackMaster>> GetAll();
        Task<List<CallbackMaster>> GetByUserID(int userId);
        Task<CallbackMaster?> GetByID(int id);
        Task<int> Insert(CallbackMaster entity);
        Task<int> Update(CallbackMaster entity);
        Task<int> Delete(int id);
    }
}
