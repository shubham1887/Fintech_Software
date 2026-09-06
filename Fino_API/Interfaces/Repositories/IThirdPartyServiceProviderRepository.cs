using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IThirdPartyServiceProviderRepository
    {
        Task<List<ThirdPartyServiceProvider>> GetAll();
        Task<ThirdPartyServiceProvider?> GetByID(int id);

        Task<int> Insert(ThirdPartyServiceProvider entity);
        Task<int> Update(ThirdPartyServiceProvider entity);
        Task<int> Delete(int id);
    }
}
