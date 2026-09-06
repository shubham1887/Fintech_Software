using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderOperatorCircleMasterRepository
    {
        Task<List<ProviderOperatorCircleMaster>> GetAll();
        Task<ProviderOperatorCircleMaster?> GetByID(int id);

        Task<int> Insert(ProviderOperatorCircleMaster entity);
        Task<int> Update(ProviderOperatorCircleMaster entity);
        Task<int> Delete(int id);
    }
}
