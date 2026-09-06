using FintechApp.Requests.BankRequestResponseLog;
using FintechApp.Responses.BankRequestResponseLog;
namespace FintechApp.Interfaces.Services
{
    public interface IBankRequestResponseLogService
    {
        Task<List<BankRequestResponseLogResponse>> GetAll();
        Task<BankRequestResponseLogResponse?> GetByID(int id);
        Task<List<BankRequestResponseLogResponse>> GetByUserID(int userid);
        Task<string> Create(CreateBankRequestResponseLogRequest request);
        Task<string> Update(UpdateBankRequestResponseLogRequest request);
        Task<string> Delete(int id);
    }
}
