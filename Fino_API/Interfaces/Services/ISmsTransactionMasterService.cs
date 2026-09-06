using FintechApp.Requests.SmsTransactionMaster;
using FintechApp.Responses.SmsTransactionMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ISmsTransactionMasterService
    {
        Task<List<SmsTransactionMasterResponse>> GetAll();
        Task<SmsTransactionMasterResponse?> GetByID(int id);

        Task<string> Create(CreateSmsTransactionMasterRequest request);
        Task<string> Update(UpdateSmsTransactionMasterRequest request);
        Task<string> Delete(int id);
    }
}
