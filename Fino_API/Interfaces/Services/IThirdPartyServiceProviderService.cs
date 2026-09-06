using FintechApp.Requests.ThirdPartyServiceProvider;
using FintechApp.Responses.ThirdPartyServiceProvider;
namespace FintechApp.Interfaces.Services
{
    public interface IThirdPartyServiceProviderService
    {
        Task<List<ThirdPartyServiceProviderResponse>> GetAll();
        Task<ThirdPartyServiceProviderResponse?> GetByID(int id);

        Task<string> Create(CreateThirdPartyServiceProviderRequest request);
        Task<string> Update(UpdateThirdPartyServiceProviderRequest request);
        Task<string> Delete(int id);
    }
}
