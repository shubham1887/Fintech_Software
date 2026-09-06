using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderOperatorDenominationMappingRepository
    {
        Task<List<ProviderOperatorDenominationMapping>> GetAll();
        Task<ProviderOperatorDenominationMapping?> GetByID(int id);
        Task<List<ProviderOperatorDenominationMapping>> GetByProviderID(int providerID);
        Task<List<ProviderOperatorDenominationMapping>> GetByOperatorID(int operatorID);
        Task<int> Insert(ProviderOperatorDenominationMapping entity);
        Task<int> Update(ProviderOperatorDenominationMapping entity);
        Task<int> Delete(int id);
    }
}
