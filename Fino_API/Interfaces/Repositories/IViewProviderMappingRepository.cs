using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IViewProviderMappingRepository
    {
        Task<List<ViewProviderMapping>> GetAll();
        Task<ViewProviderMapping?> GetByOperatorID(int operatorID);
        Task<int> GetViewProviderID(int operatorID);
        Task<int> Upsert(int operatorID, int providerID);
    }
}
