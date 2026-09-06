using FintechApp.Requests.WalletServiceMapping;
using FintechApp.Responses.WalletServiceMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IWalletServiceMappingService
    {
        Task<List<WalletServiceMappingResponse>> GetAll();
        Task<WalletServiceMappingResponse?> GetByID(int id);

        Task<string> Create(CreateWalletServiceMappingRequest request);
        Task<string> Update(UpdateWalletServiceMappingRequest request);
        Task<string> Delete(int id);
    }
}
