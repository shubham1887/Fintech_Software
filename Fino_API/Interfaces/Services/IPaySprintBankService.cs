using FintechApp.Requests.PaySprintBank;
using FintechApp.Responses.PaySprintBank;
namespace FintechApp.Interfaces.Services
{
    public interface IPaySprintBankService
    {
        Task<List<PaySprintBankResponse>> GetAll();
        Task<PaySprintBankResponse?> GetByID(int id);

        Task<string> Create(CreatePaySprintBankRequest request);
        Task<string> Update(UpdatePaySprintBankRequest request);
        Task<string> Delete(int id);
    }
}
