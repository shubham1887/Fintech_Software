using FintechApp.Requests.LowBalanceConfiguration;
using FintechApp.Responses.LowBalanceConfiguration;

namespace FintechApp.Interfaces.Services
{
    public interface ILowBalanceConfigurationService
    {
        Task<List<LowBalanceConfigurationResponse>> GetAll();
        Task<List<LowBalanceConfigurationResponse>> GetByUserID(int userID);
        Task<LowBalanceConfigurationResponse?> GetByID(int id);
        Task<string> Create(CreateLowBalanceConfigurationRequest request);
        Task<string> Update(UpdateLowBalanceConfigurationRequest request);
        Task<string> Delete(int id);
    }
}
