using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBrowserUserAgentMasterRepository
    {
        Task<List<BrowserUserAgentMaster>> GetAll();
        Task<BrowserUserAgentMaster?> GetByID(int id);
        Task<int> Insert(BrowserUserAgentMaster entity);
        Task<int> Update(BrowserUserAgentMaster entity);
        Task<int> Delete(int id);
    }
}
